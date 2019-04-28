using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TSClientGen.Extensibility;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class CustomTypeConverterTests
	{
		[Test]
		public void Types_from_custom_type_converter_take_highest_priority()
		{
			var customConverter = new CustomTypeConverter((typeof(Guid), "MyCustomGuid"));
			var mapping = new TypeMapping(customConverter);
			
			Assert.AreEqual("MyCustomGuid", mapping.GetTSType(typeof(Guid)));
		}

		[Test]
		public void Types_from_custom_type_converter_are_used_for_nested_properties()
		{
			var customConverter = new CustomTypeConverter((typeof(Guid), "MyCustomGuid"));
			var mapping = new TypeMapping(customConverter);
			mapping.GetTSType(typeof(ModelWithProp));
			var generatedType = mapping.GetGeneratedTypes()[typeof(ModelWithProp)];
			
			TextAssert.ContainsLine("identifier: MyCustomGuid;", generatedType);
		}
		
		[Test]
		public void Generic_types_from_custom_type_converter_are_supported()
		{
			var customConverter = new CustomTypeConverter((typeof(GenericModel<string>), "GenericModel"));
			var mapping = new TypeMapping(customConverter);

			Assert.AreEqual("GenericModel", mapping.GetTSType(typeof(GenericModel<string>)));
		}

		[Test]
		public void Custom_type_converter_calling_builtin_convert_and_returning_behave_in_same_way()
		{
			var mappings = new[]
			{
				new TypeMapping(new PlainTypeConverter(true)),
				new TypeMapping(new PlainTypeConverter(false))
			};

			foreach (var mapping in mappings)
			{
				Assert.AreEqual(
					"ModelWithProp",
					mapping.GetTSType(typeof(ModelWithProp)));
				CollectionAssert.AreEquivalent(
					new[] { typeof(ModelWithProp )},
					mapping.GetGeneratedTypes().Keys);
			}
		}
		
		
		class CustomTypeConverter : ITypeConverter
		{
			private readonly Dictionary<Type, string> _mappings;

			public CustomTypeConverter(params (Type type, string typescriptType)[] mappings)
			{
				_mappings = mappings.ToDictionary(p => p.type, p => p.typescriptType);
			}
			
			public string Convert(Type type, Func<Type, string> builtInConvert)
			{
				return _mappings.TryGetValue(type, out var result) ? result : null;
			}
		}

		class PlainTypeConverter : ITypeConverter
		{
			private readonly bool _callBuiltinConvert;

			public PlainTypeConverter(bool callBuiltinConvert)
			{
				_callBuiltinConvert = callBuiltinConvert;
			}

			public string Convert(Type type, Func<Type, string> builtInConvert)
			{
				return _callBuiltinConvert ? builtInConvert(type) : null;
			}
		}
		
		class GenericModel<T>
		{
		}

		class ModelWithProp
		{
			public Guid Identifier { get; }
		}
	}
}