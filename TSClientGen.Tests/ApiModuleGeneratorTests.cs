using System;
using NUnit.Framework;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class ApiModuleGeneratorTests
	{
		[Test]
		public void Adding_type_to_mapping_recursively_adds_all_referenced_types()
		{
			var mapping = new TypeMapping();
			mapping.AddType(typeof(Model));
			
			CollectionAssert.AreEquivalent(
				new[] { typeof(Model), typeof(NestedModel) },
				mapping.GetGeneratedTypes().Keys);
			CollectionAssert.AreEquivalent(
				new[] { typeof(Enum1), typeof(Enum2) },
				mapping.GetEnums());
		}

		[Test]
		public void All_nested_types_are_written_to_module()
		{
			var mapping = new TypeMapping();
			mapping.AddType(typeof(Model));

			var generator = createGenerator(mapping);
			generator.WriteTypeDefinitions();

			var result = generator.GetResult();
			TextAssert.ContainsLine("export interface Model {", result);
			TextAssert.ContainsLine("export interface NestedModel {", result);
			
			CollectionAssert.AreEquivalent(
				new[] { typeof(Model), typeof(NestedModel) },
				mapping.GetGeneratedTypes().Keys);
			CollectionAssert.AreEquivalent(
				new[] { typeof(Enum1), typeof(Enum2) },
				mapping.GetEnums());
		}

		[Test]
		public void Enum_imports_are_generated()
		{
			var mapping = new TypeMapping();
			mapping.AddType(typeof(Enum1));
			mapping.AddType(typeof(Enum2));

			var generator = createGenerator(mapping);
			generator.WriteEnumImports("enums");
			
			TextAssert.ContainsLine("import { Enum1, Enum2 } from './enums'", generator.GetResult());
		}

		
		private ApiModuleGenerator createGenerator(TypeMapping typeMapping)
		{
			var module = new ApiClientModule("client", "client", new ApiMethod[0], typeof(Controller));
			return new ApiModuleGenerator(
				module,
				typeMapping,
				(val) => throw new NotImplementedException(),
				"common");
		}
		
		
		class Controller {}
		
		class Model
		{
			public Enum1 EnumProp { get; }
			public NestedModel Nested { get; }
		}

		class NestedModel
		{
			public Enum2 EnumProp { get; } 			
		}

		enum Enum1 { A, B }

		enum Enum2 { C, D }
	}
}