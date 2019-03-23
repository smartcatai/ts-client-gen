using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TSClientGen.ApiDescriptors;
using TSClientGen.Extensibility;

namespace TSClientGen
{
	/// <summary>
	/// Stores a mapping from .net types to corresponding TypeScript types
	/// </summary>
	public class TypeMapping
	{
		public TypeMapping(ICustomTypeConverter customTypeConverter)
		{
			var defaultTypeConverter = new DefaultTypeConverter();
			_defaultTypeConverter = type => defaultTypeConverter.Convert(type, this);
			_customTypeConverter = customTypeConverter;
			_typesToGenerate = new Dictionary<Type, BaseTypeDescriptor>();
		}

		
		public string GetTSType(Type type)
		{
			var mapping = type.GetCustomAttributes<TSSubstituteTypeAttribute>().FirstOrDefault();
			if (mapping?.SubstituteType != null)
				type = mapping.SubstituteType;
			
			return _customTypeConverter?.Convert(type, _defaultTypeConverter) ?? _defaultTypeConverter(type);
		}

		public string GetTSType(PropertyInfo propertyInfo)
		{
			var mapping = propertyInfo.GetCustomAttributes<TSSubstituteTypeAttribute>().FirstOrDefault();
			if (mapping == null)
				return GetTSType(propertyInfo.PropertyType);

			if (mapping.SubstituteType != null)
				return GetTSType(mapping.SubstituteType);
			
			throw new InvalidOperationException(
				$"Invalid {nameof(TSSubstituteTypeAttribute)} on property: {propertyInfo.Name} in " + 
				$"{propertyInfo.DeclaringType?.FullName}: it is permissible to use only with a {nameof(mapping.SubstituteType)}");
		}
		
		public void AddType(Type type)
		{
			if (type == null || _typesToGenerate.ContainsKey(type))
				return;
					
			var substituteTypeMapping = type.GetCustomAttributes<TSSubstituteTypeAttribute>().FirstOrDefault();
			var polymorphicTypeMapping = type.GetCustomAttributes<TSPolymorphicTypeAttribute>().FirstOrDefault();
			var baseType = tryGetBaseType(type);
		
			if (substituteTypeMapping != null && polymorphicTypeMapping != null)
				throw new InvalidOperationException(
					$"Type can't be decorated with both {nameof(TSSubstituteTypeAttribute)} and {nameof(TSPolymorphicTypeAttribute)}");
			
			if (substituteTypeMapping == null && polymorphicTypeMapping == null)
			{
				_typesToGenerate.Add(type, new InterfaceDescriptor(type, baseType));
				AddType(baseType);
				return;
			}

			if (substituteTypeMapping != null)
			{
				if (substituteTypeMapping.SubstituteType != null)
				{
					AddType(substituteTypeMapping.SubstituteType);
				}
				else if (!string.IsNullOrWhiteSpace(substituteTypeMapping.TypeDefinition))
				{
					_typesToGenerate.Add(type, new HandwrittenTypeDescriptor(type, substituteTypeMapping.TypeDefinition));
				}
			}

			if (polymorphicTypeMapping != null)
			{
				if (polymorphicTypeMapping.SuppressDiscriminatorGeneration)
				{
					_typesToGenerate.Add(type, new InterfaceDescriptor(type, baseType));
				}
				else
				{
					_typesToGenerate.Add(type, new InterfaceDescriptor(type, polymorphicTypeMapping.DiscriminatorFieldType, polymorphicTypeMapping.DiscriminatorFieldName, baseType));					
				}
				
				AddType(baseType);
				
				var targetAsm = polymorphicTypeMapping.DescendantsAssembly ?? type.Assembly;
				var inheritedTypes = targetAsm.GetTypes().Where(type.IsAssignableFrom);
				
				foreach (var inheritedType in inheritedTypes)
				{
					AddType(inheritedType);
				}
			}
		}

		public BaseTypeDescriptor GetDescriptorByType(Type type)
		{
			return _typesToGenerate[type];
		}

		public HashSet<Type> GetTypesToGenerate() => new HashSet<Type>(_typesToGenerate.Keys);

		
		private Type tryGetBaseType(Type type)
		{
			return type.BaseType != typeof(Object) && type.BaseType != typeof(ValueType) && type.BaseType != typeof(Enum)
				? type.BaseType : null;
		}
		
		private readonly ICustomTypeConverter _customTypeConverter;
		private readonly Func<Type, string> _defaultTypeConverter;
		private readonly Dictionary<Type, BaseTypeDescriptor> _typesToGenerate;		
	}
}