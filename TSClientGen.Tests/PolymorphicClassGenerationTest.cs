using System;
using System.Text;
using NUnit.Framework;
using TSClientGen.ApiDescriptors;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class PolymorphicClassGenerationTest
	{
		[Test]
		public void Should_generate_polymorphic_interface()
		{
			var typeMapping = new TypeMapping(null);
			typeMapping.AddType(typeof(SimpleTestModel));
			var descriptor = (InterfaceDescriptor)typeMapping.GetDescriptorByType(typeof(SimpleTestModel));

			var generator = createGenerator(typeMapping);
			Assert.DoesNotThrow(() => generator.WriteInterface(descriptor));
		}
		
		 [Test]
		public void Should_generate_interface_if_type_suppresses_discriminator_generation()
		{
			var typeMapping = new TypeMapping(null);
			typeMapping.AddType(typeof(TestModelWithOwnDiscriminator));
			var descriptor = (InterfaceDescriptor)typeMapping.GetDescriptorByType(typeof(TestModelWithOwnDiscriminator));

			var generator = createGenerator(typeMapping);
			Assert.DoesNotThrow(() => generator.WriteInterface(descriptor));
		}
		
		[Test]
		public void Should_throw_exception_if_discriminator_name_equals_model_property_name()
		{
			var typeMapping = new TypeMapping(null);
			typeMapping.AddType(typeof(InvalidTestModel));
			var descriptor = (InterfaceDescriptor)typeMapping.GetDescriptorByType(typeof(InvalidTestModel));

			var generator = createGenerator(typeMapping);
			Assert.Throws<InvalidOperationException>(() => generator.WriteInterface(descriptor));
		}        
		

		private ApiClientModuleGenerator createGenerator(TypeMapping typeMapping)
		{
			var module = new ModuleDescriptor("module", "module", new MethodDescriptor[0], typeMapping, false);
			return new ApiClientModuleGenerator(module, new DefaultPropertyNameProvider(), v => v.ToString(), null); 
		}		
	}
	
	[TSPolymorphicType(typeof(TestEnum))]
	class SimpleTestModel
	{
	}
	
	[TSPolymorphicType(suppressDiscriminatorGeneration: true)]
	class TestModelWithOwnDiscriminator
	{
		public string Type { get; set; }
	}

	[TSPolymorphicType(typeof(TestEnum), typeof(InvalidTestModel), "type")]
	class InvalidTestModel
	{
		public string Type { get; set; }
	}
	
	enum TestEnum { A, B }
}