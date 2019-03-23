using System;
using NUnit.Framework;
using TSClientGen.ApiDescriptors;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class ApiModuleGeneratorTests
	{
		[Test]
		public void Writing_interface_adds_nested_types_to_mapping()
		{
			var mapping = new TypeMapping(null);
			mapping.AddType(typeof(Model));

			var generator = createGenerator(mapping);
			generator.WriteInterface((InterfaceDescriptor)mapping.GetDescriptorByType(typeof(Model)));
			
			CollectionAssert.AreEquivalent(
				new[] { typeof(Model), typeof(Enum1), typeof(NestedModel) },
				mapping.GetTypesToGenerate());
		}

		[Test]
		public void All_nested_types_are_written_to_module()
		{
			var mapping = new TypeMapping(null);
			mapping.AddType(typeof(Model));

			var generator = createGenerator(mapping);
			generator.WriteTypeDefinitions();

			var result = generator.GetResult();
			TextAssert.ContainsLine("export interface IModel {", result);
			TextAssert.ContainsLine("export interface INestedModel {", result);
			
			CollectionAssert.AreEquivalent(
				new[] { typeof(Model), typeof(Enum1), typeof(NestedModel), typeof(Enum2) },
				mapping.GetTypesToGenerate());			
		}

		[Test]
		public void Enum_imports_are_generated()
		{
			var mapping = new TypeMapping(null);
			mapping.AddType(typeof(Enum1));
			mapping.AddType(typeof(Enum2));

			var generator = createGenerator(mapping);
			generator.WriteEnumImports("enums");
			
			TextAssert.ContainsLine("import { Enum1, Enum2 } from './enums'", generator.GetResult());
		}

		
		private ApiModuleGenerator createGenerator(TypeMapping typeMapping)
		{
			var module = new ModuleDescriptor("client", new MethodDescriptor[0], typeof(Controller));
			return new ApiModuleGenerator(
				module,
				typeMapping,
				new DefaultPropertyNameProvider(),
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
	}
}