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

		[Test]
		public void Type_substitutions_on_properties_are_processed_correctly()
		{
			var mapping = new TypeMapping();

			mapping.GetTSType(typeof(ModelWithSubstitutedProps));
			var generatedType = mapping.GetGeneratedTypes()[typeof(ModelWithSubstitutedProps)];
			
			// this is not a mistake - when TSSubstituteType is applied on a property,
			// inline is ignored and always treated as true cause a single property on some type
			// should not affect property type's definition outside of this parent type
			TextAssert.ContainsLine("typedefSeparate: string | number;", generatedType);
			
			TextAssert.ContainsLine("typedefInline: string | number;", generatedType);
			TextAssert.ContainsLine("noAttribute: SimpleModel;", generatedType);
			TextAssert.ContainsLine("stringSubst: string;", generatedType);
		}
		

		class SimpleModel
		{
		}

		class ModelWithSubstitutedProps
		{
			[TSSubstituteType("string | number")]
			public SimpleModel TypedefSeparate { get; }
			
			[TSSubstituteType("string | number", true)]
			public SimpleModel TypedefInline { get; }
			
			public SimpleModel NoAttribute { get; }
			
			[TSSubstituteType(typeof(string))]
			public SimpleModel StringSubst { get; }			
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