using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSClientGen
{
	internal sealed class DefaultTypeConverter
	{
		public string Convert(Type type, TypeMapping typeMapping)
		{
			return tryMapPrimitiveType(type) ??
			       tryMapDictionary(type, typeMapping) ??
			       tryMapEnumerable(type, typeMapping) ??
			       tryMapKnownGenerics(type, typeMapping) ??
			       mapCustomType(type, typeMapping);
		}
		
		
		private string tryMapPrimitiveType(Type type)
		{
			return _primitiveTypes.TryGetValue(type, out var value) ? value : null;
		}
		
		private string tryMapDictionary(Type type, TypeMapping typeMapping)
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

			var keyTsType = Convert(keyType.IsEnum ? typeof(int) : keyType, typeMapping);			
			if (keyTsType != "number" && keyTsType != "string")
			{
				throw new InvalidOperationException($"In TS only string and number can be used as index param. Can't map dictionary key: {keyType}");
			}

			return $"{{ [id: {keyTsType}]: {Convert(valueType, typeMapping)}; }}";
		}
		
		private string tryMapEnumerable(Type type, TypeMapping typeMapping)
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

			return Convert(elementType, typeMapping) + "[]";
		}
		
		private string tryMapKnownGenerics(Type type, TypeMapping typeMapping)
		{
			if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(Nullable<>) ||
				    type.GetGenericTypeDefinition() == typeof(Task<>))
				{
					return Convert(type.GetGenericArguments()[0], typeMapping);
				}
			}

			return null;
		}

		private string mapCustomType(Type type, TypeMapping typeMapping)
		{
			if (type.IsGenericType)
				throw new InvalidOperationException($"Can't map generic type definition: {type}");

			typeMapping.AddType(type);

			return type.IsEnum ? type.Name : composeCustomTypeName(type);
		}
		
		private string composeCustomTypeName(Type type)
		{
			if (!type.IsGenericType)
				return "I" + type.Name;

			var specSymbolIndex = type.Name.IndexOf("`", StringComparison.Ordinal);
			var genericArgs = type.GetGenericArguments().Select(composeCustomTypeName);
			return $"I{type.Name.Substring(0, specSymbolIndex)}_{string.Join("_", genericArgs)}";
		}
		
		
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