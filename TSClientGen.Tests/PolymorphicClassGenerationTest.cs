using System;
using System.Text;
using NUnit.Framework;

namespace TSClientGen.Tests
{
    [TestFixture]
    public class PolymorphicClassGenerationTest
    {
        [Test]
        public void Should_generate_polymorphic_interface()
        {
            var mapper = new TypeMapper();
            mapper.AddType(typeof(SimpleTestModel));
            var descriptor = mapper.GetDescriptorByType(typeof(SimpleTestModel));
            Assert.DoesNotThrow(() => TSGenerator.GenerateInterface(new StringBuilder(), descriptor, mapper, false));
        }
        
         [Test]
        public void Should_generate_interface_if_type_suppresses_discrimanator_generation()
        {
            var mapper = new TypeMapper();
            mapper.AddType(typeof(TestModelWithOwnDiscriminator));
            var descriptor = mapper.GetDescriptorByType(typeof(TestModelWithOwnDiscriminator));
            Assert.DoesNotThrow(() => TSGenerator.GenerateInterface(new StringBuilder(), descriptor, mapper, false));
        }
        
        [Test]
        public void Should_throw_exception_if_discriminator_name_equals_model_property_name()
        {
            var mapper = new TypeMapper();
            mapper.AddType(typeof(InvalidTestModel));
            var descriptor = mapper.GetDescriptorByType(typeof(InvalidTestModel));
            Assert.Throws<InvalidOperationException>(() => TSGenerator.GenerateInterface(new StringBuilder(), descriptor, mapper, false));
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