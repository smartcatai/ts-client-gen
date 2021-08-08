// json property annotations are covered by another test,
// so just assume that Newtonsoft.Json.Required
// is mapped 1-1 to all (isNullable, isOptional) combinations by the handler

using Newtonsoft.Json;
using NUnit.Framework;

namespace TSClientGen.Tests.NullabilityIntegration
{
    [TestFixture]
    public class NullabilityCodeGenerationTest
    {
        private class TestCase
        {
            [JsonProperty(Required = Required.Always)]
            public string RequiredProp { get; set; }
            
            [JsonProperty(Required = Required.AllowNull)]
            public string NullableProp { get; set; }
            
            [JsonProperty(Required = Required.DisallowNull)]
            public string OptionalProp { get; set; }
            
            [JsonProperty(Required = Required.Default)]
            public string NullableOptionalProp { get; set; }
        }

        [Test]
        public void Test()
        {
            var t = typeof(TestCase);
            var nullabilityHandling = NullabilityHandling.JsonProperty;
            var mappingConfig = new TypeMappingConfig(nullabilityHandling);
            var mapping = new TypeMapping(config: mappingConfig);
            mapping.AddType(t);
            var converted = mapping.GetGeneratedTypes()[t];
            Assert.That(converted, Is.EqualTo(
@"export interface TestCase {
	requiredProp: string;
	nullableProp: string | null;
	optionalProp?: string;
	nullableOptionalProp?: string | null;
}
"));
        }
    }
}