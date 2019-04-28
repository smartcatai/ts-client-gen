using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class TypeSubstitutionTests
	{
		[Test]
		public void Types_from_custom_type_converter_take_highest_priority()
		{
			var customConverter = new CustomTypeConverter((typeof(Guid), "MyCustomGuid"));
			var mapping = new TypeMapping(customConverter, null);
			
			Assert.AreEqual("MyCustomGuid", mapping.GetTSType(typeof(Guid)));
		}
		
		[Test]
		public void Generic_types_from_custom_type_converter_are_supported()
		{
			var customConverter = new CustomTypeConverter((typeof(GenericModel<string>), "GenericModel"));
			var mapping = new TypeMapping(customConverter, null);

			Assert.AreEqual("GenericModel", mapping.GetTSType(typeof(GenericModel<string>)));
		}

		[Test]
		public void Type_can_be_substituted_by_another_type()
		{
			Assert.AreEqual(
				"SimpleModel", 
				new TypeMapping().GetTSType(typeof(SubstitutedModel)));
		}
		
		[Test]
		public void Type_can_be_substituted_by_handwritted_TypeScript_type_definition()
		{
			var mapping = new TypeMapping();
			
			Assert.AreEqual(
				"SubstitutedTypedefModel", 
				mapping.GetTSType(typeof(SubstitutedTypedefModel)));
			Assert.AreEqual(
				$"export type SubstitutedTypedefModel = {SubstitutedTypedefModel.TypeDefinition};",
				mapping.GetGeneratedTypes()[typeof(SubstitutedTypedefModel)]);
		}

		[Test]
		public void Type_can_be_substituted_by_primitive_type()
		{
			Assert.AreEqual(
				"string[]", 
				new TypeMapping().GetTSType(typeof(List<PrimitiveSubstitutedTypeModel>)));
		}

		[Test]
		public void Infinite_loop_of_substituted_types_is_caught()
		{
			Assert.Throws<InvalidOperationException>(
			() => new TypeMapping().GetTSType(typeof(LoopedModel)));
		}


		class CustomTypeConverter : ITypeConverter
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

		[TSSubstituteType(typeof(LoopedModel))]
		class LoopedModel
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

		[TSSubstituteType(typeof(string))]
		class PrimitiveSubstitutedTypeModel
		{
		}
	}
}