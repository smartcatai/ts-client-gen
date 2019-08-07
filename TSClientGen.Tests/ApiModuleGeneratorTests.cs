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

		[Test]
		public void Api_client_options_are_saved_to_private_field()
		{
			var generator = createGenerator(new TypeMapping(), "./transport", true);
			generator.WriteApiClientClass();

			var output = generator.GetResult();
			TextAssert.ContainsLine("import { request, ApiClientOptions } from './transport';", output);
			TextAssert.ContainsLine("constructor(private options?: ApiClientOptions & { hostname?: string }) {}", output);
		}


		private ApiModuleGenerator createGenerator(TypeMapping typeMapping, string transportModuleName = "./transport", bool useApiClientOptions = false)
		{
			var module = new ApiClientModule("client", "client", new ApiMethod[0], typeof(Controller));
			return new ApiModuleGenerator(
				module,
				typeMapping,
				(val) => throw new NotImplementedException(),
				transportModuleName,
				useApiClientOptions);
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