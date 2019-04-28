using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	/// <summary>
	/// Stores a mapping from .net types to corresponding TypeScript types
	/// </summary>
	public class TypeMapping
	{
		public TypeMapping(
			ITypeConverter customTypeConverter = null,
			ITypeDescriptorProvider typeDescriptorProvider = null,
			bool appendIPrefix = false)
		{
			_customTypeConverter = customTypeConverter;
			_typeDescriptorProvider = typeDescriptorProvider;
			_appendIPrefix = appendIPrefix;
		}


		public string GetTSType(Type type)
		{
			AddType(type);

			return _typeNames[type];
		}

		public void AddType(Type type)
		{
			if (type == null || _typeNames.ContainsKey(type))
				return;

			var typeDefinition = _customTypeConverter?.Convert(type, GetTSType);
			if (typeDefinition != null)
			{
				_typeNames.Add(type, typeDefinition);
				return;
			}

			var substituteAttr = type.GetCustomAttributes<TSSubstituteTypeAttribute>().FirstOrDefault();
			if (substituteAttr != null)
			{
				addTypeWithSubstitution(type, substituteAttr);
				return;
			}

			var tsTypeName = tryMapPrimitiveType(type) ??
			             tryMapDictionary(type) ??
			             tryMapEnumerable(type) ??
			             tryMapKnownGenerics(type);
			if (tsTypeName != null)
			{
				_typeNames.Add(type, tsTypeName);
				return;
			}

			tsTypeName = composeTypeName(type);
			_typeNames.Add(type, tsTypeName);

			var descriptor = createInterfaceDescriptor(type);
			_typeDefinitions.Add(type, writeInterface(tsTypeName, descriptor));
			AddType(descriptor.BaseType);

			var requireDescendantsAttr = type.GetCustomAttributes<TSRequireDescendantTypes>().FirstOrDefault();
			if (requireDescendantsAttr != null)
			{
				var targetAsm = requireDescendantsAttr.IncludeDescendantsFromAssembly?.Assembly ?? type.Assembly;
				var inheritedTypes = targetAsm.GetTypes().Where(type.IsAssignableFrom).ToList();
				foreach (var inheritedType in inheritedTypes)
				{
					AddType(inheritedType);
				}
			}
		}

		public IReadOnlyCollection<Type> GetEnums() => _typeNames.Select(p => p.Key).Where(t => t.IsEnum).ToList();

		public IReadOnlyDictionary<Type, string> GetGeneratedTypes() => _typeDefinitions;


		private void addTypeWithSubstitution(Type type, TSSubstituteTypeAttribute substituteAttr)
		{
			if (substituteAttr.SubstituteType != null)
			{
				if (_substitutedTypes.ContainsKey(type))
				{
					throw new InvalidOperationException(
						$"Substitute type loop detected on type {type.FullName}. " +
						"Please check TSSubstituteTypeAttribute instances in your codebase and the implementation of ICustomTypeConverted in your plugin (if any)");
				}

				var substituteType = substituteAttr.SubstituteType;
				_substitutedTypes.Add(type, substituteType);
				AddType(substituteType);
				_typeNames.Add(type, _typeNames[substituteType]);
			}
			else if (substituteAttr?.TypeDefinition != null)
			{
				if (substituteAttr.Inline)
				{
					_typeNames.Add(type, substituteAttr.TypeDefinition);
				}
				else
				{
					string tsTypeName = composeTypeName(type);
					_typeNames.Add(type, tsTypeName);
					_typeDefinitions.Add(type, writeTypeDefinition(tsTypeName, substituteAttr.TypeDefinition));
				}
			}
		}
		
		private string tryMapPrimitiveType(Type type)
		{
			return _primitiveTypes.TryGetValue(type, out var value) ? value : null;
		}
		
		private string tryMapDictionary(Type type)
		{
			var dictionaryInterfaces = type
				.GetInterfaces()
				.Where(t => t.IsGenericType 
				            && (t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
				                || t.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
				.Select(t => t.GetGenericArguments())
				.ToArray();

			if (dictionaryInterfaces.GroupBy(t => t[0], t => t[1]).Count() > 1)
			{
				throw new Exception(
					$"Found implementations of {typeof(IReadOnlyDictionary<,>).FullName}, {typeof(IDictionary<,>).FullName} with different parameters");
			}
			
			if (dictionaryInterfaces.Length == 0)
				return null;

			var genericArgs = dictionaryInterfaces[0];
			var keyType = genericArgs[0];
			var valueType = genericArgs[1];

			var keyTsType = keyType.IsEnum ? "number" : tryMapPrimitiveType(keyType);			
			if (keyTsType == null)
				throw new InvalidOperationException($"In TS only string and number can be used as index param. Can't map dictionary key: {keyType}");

			return $"{{ [id: {keyTsType}]: {GetTSType(valueType)}; }}";
		}
		
		private string tryMapEnumerable(Type type)
		{
			var elementType =
				type.GetElementType() ??
				type.GetInterfaces()
					.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					.Select(t => t.GetGenericArguments()[0])
					.SingleOrDefault();

			if (elementType == null)
				return null;

			if (elementType == type)
				return null;

			return GetTSType(elementType) + "[]";
		}

		private string tryMapKnownGenerics(Type type)
		{
			if (!type.IsGenericType)
				return null;

			if (type.GetGenericTypeDefinition() == typeof(Nullable<>) ||
			    type.GetGenericTypeDefinition() == typeof(Task<>))
			{
				return GetTSType(type.GetGenericArguments()[0]);
			}

			throw new InvalidOperationException($"Can't map generic type definition: {type}");
		}

		private string composeTypeName(Type type)
		{			
			if (type.IsEnum)
				return type.Name;
			
			if (!type.IsGenericType)
				return _appendIPrefix ? "I" + type.Name : type.Name;

			var specSymbolIndex = type.Name.IndexOf("`", StringComparison.Ordinal);
			var genericArgs = type.GetGenericArguments().Select(composeTypeName).ToList();
			
			var sb = new StringBuilder();
			if (_appendIPrefix)
				sb.Append('I');
			sb.Append(type.Name.Substring(0, specSymbolIndex));
			for (var i = 0; i < genericArgs.Count; i++)
			{
				if (i > 0)
					sb.Append('_');
				sb.Append(genericArgs[i]);
			}
			return sb.ToString();
		}
		
		private TypeDescriptor createInterfaceDescriptor(Type type)
		{
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Select(createPropertyDescriptor)
				.Where(p => p != null)
				.ToList();
			var descriptor = new TypeDescriptor(type, properties);
			descriptor = _typeDescriptorProvider?.DescribeType(type, descriptor) ?? descriptor;
			return descriptor;
		}

		private TypePropertyDescriptor createPropertyDescriptor(PropertyInfo p)
		{
			if (p.GetCustomAttributes<IgnoreDataMemberAttribute>().Any())
				return null;

			var dataMember = p.GetCustomAttributes<DataMemberAttribute>().FirstOrDefault();
			var propertyName = dataMember?.Name ?? toLowerCamelCase(p.Name);

			var propertyType = p.PropertyType;
			string propertyInlineDefinition = null;
			var mapping = p.GetCustomAttributes<TSSubstituteTypeAttribute>().FirstOrDefault();
			if (mapping?.SubstituteType != null)
			{
				propertyType = mapping.SubstituteType;
			}
			else if (mapping?.TypeDefinition != null)
			{
				propertyInlineDefinition = mapping.TypeDefinition;
			}

			return new TypePropertyDescriptor(propertyName, propertyType, propertyInlineDefinition);
		}

		private string toLowerCamelCase(string name)
		{
			return char.ToLowerInvariant(name[0]) + name.Substring(1);
		}

		private string writeTypeDefinition(string tsTypeName, string typeDefinition)
		{
			return $"export type {tsTypeName} = {typeDefinition};";
		}

		private string writeInterface(string tsTypeName, TypeDescriptor typeDescriptor)
		{
			var result = new IndentedStringBuilder();
			
			result.Append($"export interface {tsTypeName} ");
			if (typeDescriptor.BaseType != null)
			{
				result.Append($"extends {GetTSType(typeDescriptor.BaseType)} ");
			}
			
			result.AppendLine("{").Indent();
			
			foreach (var property in typeDescriptor.Properties)
			{
				if (property.InlineTypeDefinition == null)
				{
					var name = property.Name;
					if (Nullable.GetUnderlyingType(property.Type) != null)
						name += "?";

					result.AppendLine($"{name}: {GetTSType(property.Type)};");
				}
				else
				{
					result.AppendLine($"{property.Name}: {property.InlineTypeDefinition};");					
				}
			}

			result.Unindent().AppendLine("}");
			return result.ToString();
		}
		
		
		private readonly bool _appendIPrefix;
		private readonly ITypeConverter _customTypeConverter;
		private readonly ITypeDescriptorProvider _typeDescriptorProvider;

		private readonly Dictionary<Type, string> _typeDefinitions = new Dictionary<Type, string>();
		private readonly Dictionary<Type, string> _typeNames = new Dictionary<Type, string>();
		private readonly Dictionary<Type, Type> _substitutedTypes = new Dictionary<Type, Type>();
		
		private static readonly IReadOnlyDictionary<Type, string> _primitiveTypes = new Dictionary<Type, string>
		{
			{ typeof(bool),		"boolean" },
			{ typeof(DateTime),	"Date" },
			{ typeof(object),	"any" },
			{ typeof(void),		"void" },
			{ typeof(Task),		"void" },
			{ typeof(string),	"string" },
			{ typeof(byte),		"number" },
			{ typeof(short),	"number" },
			{ typeof(int),		"number" },
			{ typeof(long),		"number" },
			{ typeof(ushort),	"number" },
			{ typeof(uint),		"number" },
			{ typeof(ulong),	"number" },
			{ typeof(float),	"number" },
			{ typeof(double),	"number" },
			{ typeof(decimal),	"number" },
			{ typeof(Guid),		"string" }
		};		
	}
}