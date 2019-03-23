using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TSClientGen.ApiDescriptors;
using TSClientGen.Extensibility;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class TypeMappingTests
	{
		[Test]
		public void Types_from_custom_type_converter_take_highest_priority()
		{
			var customConverter = new CustomTypeConverter((typeof(Guid), "MyCustomGuid"));
			var mapping = new TypeMapping(customConverter);
			
			Assert.AreEqual("MyCustomGuid", mapping.GetTSType(typeof(Guid)));
		}
		
		[Test]
		public void Generic_types_from_custom_type_converter_are_supported()
		{
			var customConverter = new CustomTypeConverter((typeof(GenericModel<string>), "GenericModel"));
			var mapping = new TypeMapping(customConverter);

			Assert.AreEqual("GenericModel", mapping.GetTSType(typeof(GenericModel<string>)));
		}

		[Test]
		public void Custom_complex_types_are_stored_in_type_mapping_as_interfaces()
		{
			var mapping = new TypeMapping(null);

			Assert.AreEqual("ISimpleModel", mapping.GetTSType(typeof(SimpleModel)));
			Assert.IsInstanceOf(typeof(InterfaceDescriptor), mapping.GetDescriptorByType(typeof(SimpleModel)));
		}

		[Test]
		public void Type_can_be_substituted_by_another_type()
		{
			Assert.AreEqual(
				"ISimpleModel", 
				new TypeMapping(null).GetTSType(typeof(SubstitutedModel)));
		}
		
		[Test]
		public void Type_can_be_substituted_by_handwritted_TypeScript_type_definition()
		{
			var mapping = new TypeMapping(null);
			
			Assert.AreEqual(
				"ISubstitutedTypedefModel", 
				mapping.GetTSType(typeof(SubstitutedTypedefModel)));
			Assert.AreEqual(
				SubstitutedTypedefModel.TypeDefinition,
				((HandwrittenTypeDescriptor)mapping.GetDescriptorByType(typeof(SubstitutedTypedefModel))).TypeDefinition);
		}


		class CustomTypeConverter : ICustomTypeConverter
		{
			private readonly Dictionary<Type, string> _mappings;

			public CustomTypeConverter(params (Type type, string typescriptType)[] mappings)
			{
				_mappings = mappings.ToDictionary(p => p.type, p => p.typescriptType);
			}
			
			public string Convert(Type type, Func<Type, string> defaultConvert)
			{
				return _mappings.TryGetValue(type, out var result) ? result : defaultConvert(type);
			}
		}
		
		class GenericModel<T>
		{
		}

		class SimpleModel
		{
		}

		[TSSubstituteType(typeof(SimpleModel))]
		class SubstitutedModel
		{
		}
		
		[TSSubstituteType(TypeDefinition)]
		class SubstitutedTypedefModel
		{
			public const string TypeDefinition = "{ mySecretContents: string }";
		}
	}
}