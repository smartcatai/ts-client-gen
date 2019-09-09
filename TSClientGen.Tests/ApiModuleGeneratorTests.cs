using System;
using NUnit.Framework;
using TSClientGen.Extensibility;
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
		public void Should_write_custom_imports_if_api_generation_extensions_provided()
		{
			var mapping = new TypeMapping();
			var customWriterMock = new CustomApiWriter();
			var generator = createGenerator(mapping, customWriterMock);

			generator.WriteApiClientClass();

			TextAssert.ContainsLine("import { foo } from 'bar'", generator.GetResult());
		}

		[Test]
		public void Should_write_custom_prefix_if_api_generation_extensions_provided()
		{
			var mapping = new TypeMapping();
			var customWriterMock = new CustomApiWriter();
			var generator = createGenerator(mapping, customWriterMock);

			generator.WriteApiClientClass();

			TextAssert.ContainsLine("foo.before();", generator.GetResult());
		}

		[Test]
		public void Should_write_custom_suffix_if_api_generation_extensions_provided()
		{
			var mapping = new TypeMapping();
			var customWriterMock = new CustomApiWriter();
			var generator = createGenerator(mapping, customWriterMock);

			generator.WriteApiClientClass();

			TextAssert.ContainsLine("foo.after();", generator.GetResult());
		}

		private ApiModuleGenerator createGenerator(TypeMapping typeMapping, IApiClientWriter customWriter = null)
		{
			var module = new ApiClientModule("client", new ApiMethod[0], typeof(Controller));
			return new ApiModuleGenerator(
				module,
				typeMapping,
				customWriter,
				(val) => throw new NotImplementedException(),
				"common");
		}
		
		
		[TSModule("api")]
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

		class CustomApiWriter : IApiClientWriter
		{
			public void WriteImports(IIndentedStringBuilder builder)
			{
				builder.AppendLine("import { foo } from 'bar'");
			}

			public void WriteCodeBeforeApiClientClass(IIndentedStringBuilder builder)
			{
				builder.AppendLine("foo.before();");
			}

			public void WriteCodeAfterApiClientClass(IIndentedStringBuilder builder)
			{
				builder.AppendLine("foo.after();");
			}
		}
	}
}