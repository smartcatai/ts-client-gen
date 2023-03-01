using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;
using TSClientGen.Nullability;

namespace TSClientGen
{
	/// <summary>
	/// Stores a mapping from .net types to corresponding TypeScript types
	/// </summary>
	public class TypeMapping
	{
		public TypeMapping(
			ITypeConverter customTypeConverter = null,
			IEnumerable<ITypeDescriptorProvider> typeDescriptorProviders = null,
			bool appendIPrefix = false,
			TypeMappingConfig config = default)
		{
			_customTypeConverter = customTypeConverter;
			_typeDescriptorProviders = typeDescriptorProviders?.ToArray() ?? Array.Empty<ITypeDescriptorProvider>();
			_appendIPrefix = appendIPrefix;
			_config = config;
			_nullabilityHandler = NullabilityHandlerResolver.FromConfig(config);
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

			// This is to protect against custom ITypeConverter implementation calling builtInConvert
			// on original type instead of returning null when it does not have mapping for a requested type.
			// Adding an entry with null before calling into custom ITypeConverter implementation
			// prevents the infinite loop between alternating GetTSType and AddType calls.
			_typeNames.Add(type, null);
			
			var typeDefinition = _customTypeConverter?.Convert(type, GetTSType);
			if (typeDefinition != null)
			{
				_typeNames[type] = typeDefinition;
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
				_typeNames[type] = tsTypeName;
				return;
			}

			tsTypeName = composeTypeName(type);
			_typeNames[type] = tsTypeName;

			var descriptor = createInterfaceDescriptor(type);
			_typeDefinitions.Add(type, writeInterface(tsTypeName, descriptor));
			AddType(descriptor.BaseType);

			foreach (var requireDescendantsAttr in type.GetCustomAttributes<TSRequireDescendantTypes>())
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

		public bool IsPrimitiveTsType(Type type)
		{
			if (type.IsEnum)
			{
				return true;
			}
			
			if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(Nullable<>) ||
				    type.GetGenericTypeDefinition() == typeof(Task<>))
				{
					return IsPrimitiveTsType(type.GetGenericArguments()[0]);
				}
			}
			
			var substituteAttr = type.GetCustomAttributes<TSSubstituteTypeAttribute>().FirstOrDefault();
			if (substituteAttr != null)
			{
				return true;
			}

			if (_typeNames.TryGetValue(type, out var tsType))
			{
				if (tsType.EndsWith("[]"))
				{
					return _primitiveTsTypes.Contains(tsType.Substring(0, tsType.Length - 2));
				}
				return _primitiveTsTypes.Contains(tsType);
			}

			return false;
		}


		private void addTypeWithSubstitution(Type type, TSSubstituteTypeAttribute substituteAttr)
		{
			if (substituteAttr.SubstituteType != null)
			{
				var substituteType = substituteAttr.SubstituteType;
				AddType(substituteType);
				if (_typeNames[substituteType] == null)
				{
					// The type that we are going to use for substitution
					// is being processed somewhere up in the same call chain.
					// This means we have a loop in type substitution directives.
					throw new InvalidOperationException(
						$"Substitute type loop detected on type {type.FullName}. " +
						"Please check TSSubstituteTypeAttribute instances in your codebase and the implementation of ICustomTypeConverted in your plugin (if any)");					
				}
				
				_typeNames[type] = _typeNames[substituteType];
			}
			else if (substituteAttr.TypeDefinition != null)
			{
				if (substituteAttr.Inline)
				{
					_typeNames[type] = substituteAttr.TypeDefinition;
				}
				else
				{
					string tsTypeName = composeTypeName(type);
					_typeNames[type] = tsTypeName;
					_typeDefinitions.Add(type, writeTypeDefinition(tsTypeName, substituteAttr.TypeDefinition));
				}
			}
		}
		
		private string tryMapPrimitiveType(Type type)
		{
			return _primitiveTypes.TryGetValue(type, out var value)
				? value
				: type.IsEnum
					? type.Name : null;
		}
		
		private string tryMapDictionary(Type type)
		{
			var dictionaryInterfaces = Enumerable.Repeat(type, 1)
				.Concat(type.GetInterfaces())
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
				Enumerable.Repeat(type, 1)
					.Concat(type.GetInterfaces())
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

			return null;
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
			foreach (var typeArg in genericArgs)
			{
				sb.Append('_').Append(typeArg);
			}
			return sb.ToString();
		}
		
		private TypeDescriptor createInterfaceDescriptor(Type type)
		{
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetCustomAttribute<TSIgnoreAttribute>() == null)
				.Select(p => new { Descriptor = createPropertyDescriptor(p), Property = p })
				.Where(p => p.Descriptor != null)
				.ToList();
			var descriptor = new TypeDescriptor(type, properties.Select(p => p.Descriptor).ToList());

			var propertiesByDescriptor = new Lazy<IReadOnlyDictionary<TypePropertyDescriptor, PropertyInfo>>(
				() => properties.ToDictionary(d => d.Descriptor, d => d.Property));
			PropertyInfo GetPropertyInfo(TypePropertyDescriptor prop)
			{
				return propertiesByDescriptor.Value[prop];
			}

			foreach (var typeDescriptorProvider in _typeDescriptorProviders)
				descriptor = typeDescriptorProvider.DescribeType(type, descriptor, GetPropertyInfo);

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
				// attribute's Inline property is ignored and always treated as true here
				// cause a single property on some type should not affect property type's definition
				// outside of this parent type
				propertyType = mapping.SubstituteType;
			}
			else if (mapping?.TypeDefinition != null)
			{
				propertyInlineDefinition = mapping.TypeDefinition;
			}

			var tsZeroType = _nullabilityHandler.GetTsNullability(p);

			return new TypePropertyDescriptor(propertyName, propertyType, tsZeroType.IsNullable, tsZeroType.IsOptional,
				propertyInlineDefinition);
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
				var name = property.Name;
				if (property.IsOptional) // is short-circuited to false if no null-checking is allowed for overrides
					name += "?";
				var type = property.InlineTypeDefinition ?? GetTSType(property.Type);
				if (property.IsNullable) // is short-circuited to false if no null-checking is allowed for overrides
					type += " | null";

				result.AppendLine($"{name}: {type};");
			}

			result.Unindent().AppendLine("}");
			return result.ToString();
		}
		
		
		private readonly bool _appendIPrefix;
		private readonly TypeMappingConfig _config;
		private readonly INullabilityHandler _nullabilityHandler;
		private readonly ITypeConverter _customTypeConverter;
		private readonly IReadOnlyCollection<ITypeDescriptorProvider> _typeDescriptorProviders;

		private readonly Dictionary<Type, string> _typeDefinitions = new Dictionary<Type, string>();
		private readonly Dictionary<Type, string> _typeNames = new Dictionary<Type, string>();
		
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

		private static readonly IReadOnlyCollection<string> _primitiveTsTypes = new HashSet<string>
		{
			 "boolean",
			 "Date",
			 "any",
			 "void",
			 "string",
			 "number"
		};
	}
}
