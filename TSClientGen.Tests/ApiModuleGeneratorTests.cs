using System;
using System.Text.RegularExpressions;
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
		public void Adding_type_to_mapping_adds_all_descenand_types_if_attribute_specified()
		{
			var mapping = new TypeMapping();
			mapping.AddType(typeof(BaseClass));

			CollectionAssert.AreEquivalent(
				new[] {typeof(BaseClass), typeof(Descendant1), typeof(Descendant2)},
				mapping.GetGeneratedTypes().Keys);
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
			
			TextAssert.ContainsLine("import { Enum1 } from './enums/Enum1'", generator.GetResult());
			TextAssert.ContainsLine("import { Enum2 } from './enums/Enum2'", generator.GetResult());
		}

		[Test]
		public void Should_write_empty_constructor_by_default()
		{
			var mapping = new TypeMapping();
			var whitespaces = new Regex(@"\s");
			var generator = createGenerator(mapping);
			
			generator.WriteApiClientClass();

			var result = whitespaces.Replace(generator.GetResult(), "");
			StringAssert.Contains("constructor(){}", result);
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
		
		[Test]
		public void Should_extend_constructor_if_api_generation_extensions_provided()
		{
			var mapping = new TypeMapping();
			var customWriterMock = new CustomApiWriter();
			var generator = createGenerator(mapping, customWriterMock);

			generator.WriteApiClientClass();

			TextAssert.ContainsLine("foo.extendConstructor();", generator.GetResult());
		}
		
		[Test]
		public void Should_extend_class_body_if_api_generation_extensions_provided()
		{
			var mapping = new TypeMapping();
			var customWriterMock = new CustomApiWriter();
			var generator = createGenerator(mapping, customWriterMock);

			generator.WriteApiClientClass();

			TextAssert.ContainsLine("public bar() { return foo.bar(); }", generator.GetResult());
		}

		private ApiModuleGenerator createGenerator(TypeMapping typeMapping, IApiClientWriter customWriter = null)
		{
			var module = new ApiClientModule("client", "client", new ApiMethod[0], typeof(Controller));
			return new ApiModuleGenerator(
				module,
				typeMapping,
				customWriter,
				(val) => throw new NotImplementedException(),
				"transport");
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

		[TSRequireDescendantTypes]
		class BaseClass {}

		class Descendant1 : BaseClass {}

		class Descendant2 : BaseClass {}

		enum Enum1 { A, B }

		enum Enum2 { C, D }

		class CustomApiWriter : IApiClientWriter
		{
			public void WriteImports(IIndentedStringBuilder builder, ApiClientModule apiClientModule)
			{
				builder.AppendLine("import { foo } from 'bar'");
			}

			public void WriteCodeBeforeApiClientClass(IIndentedStringBuilder builder, ApiClientModule apiClientModule)
			{
				builder.AppendLine("foo.before();");
			}

			public void WriteCodeAfterApiClientClass(IIndentedStringBuilder builder, ApiClientModule apiClientModule)
			{
				builder.AppendLine("foo.after();");
			}

			public void ExtendApiClientConstructor(IIndentedStringBuilder builder, ApiClientModule apiClientModule)
			{
				builder.AppendLine("foo.extendConstructor();");
			}

			public void ExtendApiClientClass(IIndentedStringBuilder builder, ApiClientModule apiClientModule)
			{
				builder.AppendLine("public bar() { return foo.bar(); }");
			}
		}
	}
}