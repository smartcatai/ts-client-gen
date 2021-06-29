using Newtonsoft.Json;
using NUnit.Framework;

namespace TSClientGen.Tests.NullabilityIntegration
{
    [TestFixture]
    public class JsonPropertyHandlerTest : HandlerTestBase<JsonPropertyHandlerTest.TestCases>
    {
        protected override TypeMappingConfig Config => new TypeMappingConfig(NullabilityHandling.JsonProperty);

        public class TestCases
        {
            // ReSharper disable below InconsistentNaming
            public int Value_type__no_attribute__not_null__no_override { get; set; }

            [ExpectOptional]
            public int? Value_type__no_attribute__nullable__no_override { get; set; }
            
            public string Reference_type__no_attribute__no_override { get; set; }

            [ExpectNullable, ExpectOptional]
            [JsonProperty(Required = Required.Default)]
            public int Value_type__default__no_override { get; set; }

            [ExpectNullable, ExpectOptional]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int Value_type__default__with_override { get; set; }
            
            [JsonProperty(Required = Required.Always)]
            public int Value_type__always__no_override { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Always)]
            public int Value_type__always__with_override { get; set; }
            
            [ExpectNullable]
            [JsonProperty(Required = Required.AllowNull)]
            public int Value_type__allow_null__no_override { get; set; }

            [ExpectNullable]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.AllowNull)]
            public int Value_type__allow_null__with_override { get; set; }
            
            [ExpectOptional]
            [JsonProperty(Required = Required.DisallowNull)]
            public int Value_type__disallow_null__no_override { get; set; }

            [ExpectOptional]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.DisallowNull)]
            public int Value_type__disallow_null__with_override { get; set; }

            [ExpectNullable, ExpectOptional]
            [JsonProperty(Required = Required.Default)]
            public int? Nullable_value_type__default__no_override { get; set; }

            [ExpectNullable, ExpectOptional]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int? Nullable_value_type__default__with_override { get; set; }
            
            [JsonProperty(Required = Required.Always)]
            public int? Nullable_value_type__always__no_override { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Always)]
            public int? Nullable_value_type__always__with_override { get; set; }
            
            [ExpectNullable]
            [JsonProperty(Required = Required.AllowNull)]
            public int? Nullable_value_type__allow_null__no_override { get; set; }

            [ExpectNullable]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.AllowNull)]
            public int? Nullable_value_type__allow_null__with_override { get; set; }
            
            [ExpectOptional]
            [JsonProperty(Required = Required.DisallowNull)]
            public int? Nullable_value_type__disallow_null__no_override { get; set; }

            [ExpectOptional]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.DisallowNull)]
            public int? Nullable_value_type__disallow_null__with_override { get; set; }
            
            [ExpectNullable, ExpectOptional]
            [JsonProperty(Required = Required.Default)]
            public string Reference_type__default__no_override { get; set; }

            [ExpectNullable, ExpectOptional]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public string Reference_type__default__with_override { get; set; }
            
            [JsonProperty(Required = Required.Always)]
            public string Reference_type__always__no_override { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Always)]
            public string Reference_type__always__with_override { get; set; }
            
            [ExpectNullable]
            [JsonProperty(Required = Required.AllowNull)]
            public string Reference_type__allow_null__no_override { get; set; }

            [ExpectNullable]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.AllowNull)]
            public string Reference_type__allow_null__with_override { get; set; }
            
            [ExpectOptional]
            [JsonProperty(Required = Required.DisallowNull)]
            public string Reference_type__disallow_null__no_override { get; set; }

            [ExpectOptional]
            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.DisallowNull)]
            public string Reference_type__disallow_null__with_override { get; set; }
        }
    }
}