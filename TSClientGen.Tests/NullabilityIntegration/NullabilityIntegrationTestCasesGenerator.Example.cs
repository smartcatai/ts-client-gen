using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TSClientGen.Tests.CodeGen
{
    public static class NullabilityIntegrationTestCases
    {
        public static IEnumerable<(Type testType, string caseName, TypeMappingConfig config, string expectation)>
            EnumerateCases()
        {
            yield return (
                typeof(Test_reference_type__attr_None__allow_override),
                "Test_reference_type__attr_None__allow_override",
                new TypeMappingConfig(NullabilityHandling.Default),
                @"export interface Test_reference_type__attr_None__allow_override {
	propDefault: string;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_reference_type__attr_None__allow_override),
                "Test_nullable_reference_type__attr_None__allow_override",
                new TypeMappingConfig(NullabilityHandling.Nrt),
                @"export interface Test_nullable_reference_type__attr_None__allow_override {
	propDefault: string | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_None__allow_override),
                "Test_value_type__attr_None__allow_override",
                new TypeMappingConfig(NullabilityHandling.Default),
                @"export interface Test_value_type__attr_None__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_RequiredAttr__allow_override),
                "Test_value_type__attr_RequiredAttr__allow_override",
                new TypeMappingConfig(NullabilityHandling.DataAnnotations),
                @"export interface Test_value_type__attr_RequiredAttr__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDefault__allow_override),
                "Test_value_type__attr_JsonDefault__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_value_type__attr_JsonDefault__allow_override {
	propDefault?: number | null;
	propOverriden?: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAlways__allow_override),
                "Test_value_type__attr_JsonAlways__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_value_type__attr_JsonAlways__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAllowNull__allow_override),
                "Test_value_type__attr_JsonAllowNull__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_value_type__attr_JsonAllowNull__allow_override {
	propDefault: number | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDisallowNull__allow_override),
                "Test_value_type__attr_JsonDisallowNull__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_value_type__attr_JsonDisallowNull__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_None__allow_override),
                "Test_nullable_value_type__attr_None__allow_override",
                new TypeMappingConfig(NullabilityHandling.Default),
                @"export interface Test_nullable_value_type__attr_None__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_RequiredAttr__allow_override),
                "Test_nullable_value_type__attr_RequiredAttr__allow_override",
                new TypeMappingConfig(NullabilityHandling.DataAnnotations),
                @"export interface Test_nullable_value_type__attr_RequiredAttr__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDefault__allow_override),
                "Test_nullable_value_type__attr_JsonDefault__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_nullable_value_type__attr_JsonDefault__allow_override {
	propDefault?: number | null;
	propOverriden?: any | null;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAlways__allow_override),
                "Test_nullable_value_type__attr_JsonAlways__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_nullable_value_type__attr_JsonAlways__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAllowNull__allow_override),
                "Test_nullable_value_type__attr_JsonAllowNull__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_nullable_value_type__attr_JsonAllowNull__allow_override {
	propDefault: number | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDisallowNull__allow_override),
                "Test_nullable_value_type__attr_JsonDisallowNull__allow_override",
                new TypeMappingConfig(NullabilityHandling.JsonProperty),
                @"export interface Test_nullable_value_type__attr_JsonDisallowNull__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );
        }

        public class Test_reference_type__attr_None__allow_override
        {
            public string PropDefault { get; set; }

            [TSSubstituteType("any")] public string PropOverriden { get; set; }
        }


#nullable enable
        public class Test_nullable_reference_type__attr_None__allow_override
        {
            public string? PropDefault { get; set; }

            [TSSubstituteType("any")] public string? PropOverriden { get; set; }
        }
#nullable restore

        public class Test_value_type__attr_None__allow_override
        {
            public int PropDefault { get; set; }

            [TSSubstituteType("any")] public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_RequiredAttr__allow_override
        {
            [Required] public int PropDefault { get; set; }

            [TSSubstituteType("any")] [Required] public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_JsonDefault__allow_override
        {
            [JsonProperty(Required = Required.Default)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_JsonAlways__allow_override
        {
            [JsonProperty(Required = Required.Always)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Always)]
            public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_JsonAllowNull__allow_override
        {
            [JsonProperty(Required = Required.AllowNull)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.AllowNull)]
            public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_JsonDisallowNull__allow_override
        {
            [JsonProperty(Required = Required.DisallowNull)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.DisallowNull)]
            public int PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_None__allow_override
        {
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")] public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_RequiredAttr__allow_override
        {
            [Required] public int? PropDefault { get; set; }

            [TSSubstituteType("any")] [Required] public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_JsonDefault__allow_override
        {
            [JsonProperty(Required = Required.Default)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_JsonAlways__allow_override
        {
            [JsonProperty(Required = Required.Always)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Always)]
            public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_JsonAllowNull__allow_override
        {
            [JsonProperty(Required = Required.AllowNull)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.AllowNull)]
            public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_JsonDisallowNull__allow_override
        {
            [JsonProperty(Required = Required.DisallowNull)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.DisallowNull)]
            public int? PropOverriden { get; set; }
        }
    }
}