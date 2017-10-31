using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace TSClientGen
{
	class TypeMapper
	{
		public TypeMapper()
		{
			_customTypes = new Dictionary<Type, CustomTypeDescriptor>();
		}

		public string GetTSType(Type type)
		{
			var mapping = type.GetCustomAttributes<TypeScriptTypeAttribute>().FirstOrDefault();

			if (mapping?.SubstituteType != null)
			{
				type = mapping.SubstituteType;
			}
			
			return tryMapPrimitiveType(type) ??
			   tryMapDictionary(type) ??
			   tryMapArray(type) ??
			   tryMapKnownGenerics(type) ??
			   mapCustomType(type);
		}

		public string GetTSType(PropertyInfo propertyInfo)
		{
			var mapping = propertyInfo.GetCustomAttributes<TypeScriptTypeAttribute>().FirstOrDefault();

			if (mapping == null)
			{
				return GetTSType(propertyInfo.PropertyType);
			}

			if (mapping.SubstituteType != null)
			{
				return GetTSType(mapping.SubstituteType);
			}
			
			throw new InvalidOperationException(
				$"Invalid {nameof(TypeScriptTypeAttribute)} on property: {propertyInfo.Name} in {propertyInfo.DeclaringType?.FullName}: it is permissible to use only with a {nameof(mapping.SubstituteType)}");
		}
		
		public void AddType(Type type)
		{
			if (type == null || _customTypes.ContainsKey(type))
				return;
		
			var mapping = type.GetCustomAttributes<TypeScriptTypeAttribute>().FirstOrDefault();
			var baseType = tryGetBaseType(type);
		
			if (mapping == null)
			{
				AddType(baseType);
				_customTypes.Add(type, new CustomTypeDescriptor(type, baseType));
			}
			else if (mapping.InheritedTypes.Any())
			{
				AddType(baseType);
				_customTypes.Add(type, new CustomTypeDescriptor(type, mapping.DiscriminatorFieldType, mapping.DiscriminatorFieldName, baseType));
				foreach (var inheritedType in mapping.InheritedTypes)
				{
					AddType(inheritedType);
				}
			}
			else if (mapping.SubstituteType != null)
			{
				AddType(mapping.SubstituteType);
			}
			else if (!string.IsNullOrWhiteSpace(mapping.TypeDefinition))
			{
				_customTypes.Add(type, new CustomTypeDescriptor(type, mapping.TypeDefinition));
			}
			else
			{
				throw new InvalidOperationException(
					$"On type {type.FullName} found {nameof(TypeScriptTypeAttribute)} with empty {nameof(TypeScriptTypeAttribute.InheritedTypes)},{nameof(TypeScriptTypeAttribute.SubstituteType)},{nameof(TypeScriptTypeAttribute.TypeDefinition)}");
			}
		}

		public CustomTypeDescriptor GetDescriptorByType(Type type)
		{
			return _customTypes[type];
		}

		public HashSet<Type> GetCustomTypes() => new HashSet<Type>(_customTypes.Keys);

		private string mapCustomType(Type type)
		{
			if (type.IsGenericTypeDefinition)
				throw new Exception($"Can't map generic type definition: {type}");

			AddType(type);

			if (type.IsEnum)
				return type.Name;

			return composeCustomTypeName(type);
		}

		private string composeCustomTypeName(Type type)
		{
			if (!type.IsGenericType)
				return "I" + type.Name;

			var specSymbolIndex = type.Name.IndexOf("`", StringComparison.Ordinal);
			var genericArgs = type.GetGenericArguments().Select(composeCustomTypeName);
			return $"I{type.Name.Substring(0, specSymbolIndex)}_{string.Join("_", genericArgs)}";
		}

		private string tryMapKnownGenerics(Type type)
		{
			if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(Nullable<>) ||
					type.GetGenericTypeDefinition() == typeof(Task<>) ||
					type.Name.StartsWith("ModelWithFiles"))
				{
					return GetTSType(type.GetGenericArguments()[0]);
				}
			}

			return null;
		}

		private string tryMapDictionary(Type type)
		{
			var dictionaryInterface = type
				.GetInterfaces()
				.Concat(new[] { type })
				.SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));

			if (dictionaryInterface == null)
				return null;

			var genericArgs = dictionaryInterface.GetGenericArguments();
			var keyType = genericArgs[0];
			var valueType = genericArgs[1];

			string keyTSType;
			if (keyType == typeof(string))
			{
				keyTSType = GetTSType(keyType);
			}
			else if (keyType.IsEnum || GetTSType(keyType) == "number")
			{
				keyTSType = GetTSType(typeof(int));
			}
			else
			{
				throw new Exception($"In TS only string and number can be used as index param. Can't map dictionary key: {keyType}");
			}

			return $"{{ [id: {keyTSType}]: {GetTSType(valueType)}; }}";
		}

		private string tryMapArray(Type type)
		{
			var elementType =
				type.GetElementType() ??
				type.GetInterfaces()
					.Concat(new[] { type })
					.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					.Select(t => t.GetGenericArguments()[0])
					.SingleOrDefault();

			if (elementType == null)
				return null;

			if (elementType == type)
				return null;

			return GetTSType(elementType) + "[]";
		}

		private string tryMapPrimitiveType(Type type)
		{
			switch (type.Name)
			{
				case "JObject":
				case "JValue":
					return "any";

				case "DateTime":
					return "Date";

				case "Object":
				case "HttpResponseMessage":
					return "any";

				case "Task":
				case "Void":
					return "void";

				case "String":
					return "string";

				case "Int16":
				case "Int32":
				case "Int64":
				case "Single":
				case "Double":
				case "Decimal":
					return "number";

				case "Guid":
				case "ObjectId":
					return "string";

				case "Boolean":
					return "boolean";

				default:
					return null;
			}
		}

		private Type tryGetBaseType(Type type)
		{
			return type.BaseType != typeof(Object) && type.BaseType != typeof(ValueType) && type.BaseType != typeof(Enum) ? type.BaseType : null;
		}
		
		private readonly Dictionary<Type, CustomTypeDescriptor> _customTypes;
	}
}