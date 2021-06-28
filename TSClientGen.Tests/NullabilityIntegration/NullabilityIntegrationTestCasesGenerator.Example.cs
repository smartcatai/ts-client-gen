using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TSClientGen.Tests.CodeGen
{
    public static class NullabilityIntegrationTestCases
    {
        public static IEnumerable<(Type testType, string caseName, TypeMappingConfig config, string expectation)>
            EnumerateCases()
        {
            yield return (
                typeof(Test_reference_type__attr_None),
                "Test_reference_type__attr_None",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_reference_type__attr_None {
	propDefault: string;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_reference_type__attr_None),
                "Test_reference_type__attr_None__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_reference_type__attr_None {
	propDefault: string;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_reference_type__attr_None__allow_override),
                "Test_reference_type__attr_None__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_reference_type__attr_None__allow_override {
	propDefault: string;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_reference_type__attr_None__allow_override),
                "Test_reference_type__attr_None__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_reference_type__attr_None__allow_override {
	propDefault: string;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_reference_type__attr_None),
                "Test_nullable_reference_type__attr_None",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Nrt,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_reference_type__attr_None {
	propDefault: string | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_reference_type__attr_None),
                "Test_nullable_reference_type__attr_None__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Nrt,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_reference_type__attr_None {
	propDefault?: string | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_reference_type__attr_None__allow_override),
                "Test_nullable_reference_type__attr_None__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Nrt,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_reference_type__attr_None__allow_override {
	propDefault: string | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_nullable_reference_type__attr_None__allow_override),
                "Test_nullable_reference_type__attr_None__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Nrt,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_reference_type__attr_None__allow_override {
	propDefault?: string | null;
	propOverriden?: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_None),
                "Test_value_type__attr_None",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_None {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_None),
                "Test_value_type__attr_None__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_None {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_None__allow_override),
                "Test_value_type__attr_None__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_None__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_None__allow_override),
                "Test_value_type__attr_None__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_None__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_RequiredAttr),
                "Test_value_type__attr_RequiredAttr",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_RequiredAttr {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_RequiredAttr),
                "Test_value_type__attr_RequiredAttr__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_RequiredAttr {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_RequiredAttr__allow_override),
                "Test_value_type__attr_RequiredAttr__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_RequiredAttr__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_RequiredAttr__allow_override),
                "Test_value_type__attr_RequiredAttr__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_RequiredAttr__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDefault),
                "Test_value_type__attr_JsonDefault",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonDefault {
	propDefault?: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDefault),
                "Test_value_type__attr_JsonDefault__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonDefault {
	propDefault?: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDefault__allow_override),
                "Test_value_type__attr_JsonDefault__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonDefault__allow_override {
	propDefault?: number | null;
	propOverriden?: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDefault__allow_override),
                "Test_value_type__attr_JsonDefault__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonDefault__allow_override {
	propDefault?: number | null;
	propOverriden?: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAlways),
                "Test_value_type__attr_JsonAlways",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonAlways {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAlways),
                "Test_value_type__attr_JsonAlways__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonAlways {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAlways__allow_override),
                "Test_value_type__attr_JsonAlways__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonAlways__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAlways__allow_override),
                "Test_value_type__attr_JsonAlways__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonAlways__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAllowNull),
                "Test_value_type__attr_JsonAllowNull",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonAllowNull {
	propDefault: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAllowNull),
                "Test_value_type__attr_JsonAllowNull__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonAllowNull {
	propDefault: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAllowNull__allow_override),
                "Test_value_type__attr_JsonAllowNull__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonAllowNull__allow_override {
	propDefault: number | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonAllowNull__allow_override),
                "Test_value_type__attr_JsonAllowNull__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonAllowNull__allow_override {
	propDefault: number | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDisallowNull),
                "Test_value_type__attr_JsonDisallowNull",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonDisallowNull {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDisallowNull),
                "Test_value_type__attr_JsonDisallowNull__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonDisallowNull {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDisallowNull__allow_override),
                "Test_value_type__attr_JsonDisallowNull__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_value_type__attr_JsonDisallowNull__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_value_type__attr_JsonDisallowNull__allow_override),
                "Test_value_type__attr_JsonDisallowNull__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_value_type__attr_JsonDisallowNull__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_None),
                "Test_nullable_value_type__attr_None",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_None {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_None),
                "Test_nullable_value_type__attr_None__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_None {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_None__allow_override),
                "Test_nullable_value_type__attr_None__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_None__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_None__allow_override),
                "Test_nullable_value_type__attr_None__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.Default,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_None__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_RequiredAttr),
                "Test_nullable_value_type__attr_RequiredAttr",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_RequiredAttr {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_RequiredAttr),
                "Test_nullable_value_type__attr_RequiredAttr__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_RequiredAttr {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_RequiredAttr__allow_override),
                "Test_nullable_value_type__attr_RequiredAttr__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_RequiredAttr__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_RequiredAttr__allow_override),
                "Test_nullable_value_type__attr_RequiredAttr__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.DataAnnotations,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_RequiredAttr__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDefault),
                "Test_nullable_value_type__attr_JsonDefault",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonDefault {
	propDefault?: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDefault),
                "Test_nullable_value_type__attr_JsonDefault__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonDefault {
	propDefault?: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDefault__allow_override),
                "Test_nullable_value_type__attr_JsonDefault__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonDefault__allow_override {
	propDefault?: number | null;
	propOverriden?: any | null;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDefault__allow_override),
                "Test_nullable_value_type__attr_JsonDefault__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonDefault__allow_override {
	propDefault?: number | null;
	propOverriden?: any | null;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAlways),
                "Test_nullable_value_type__attr_JsonAlways",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonAlways {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAlways),
                "Test_nullable_value_type__attr_JsonAlways__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonAlways {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAlways__allow_override),
                "Test_nullable_value_type__attr_JsonAlways__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonAlways__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAlways__allow_override),
                "Test_nullable_value_type__attr_JsonAlways__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonAlways__allow_override {
	propDefault: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAllowNull),
                "Test_nullable_value_type__attr_JsonAllowNull",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonAllowNull {
	propDefault: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAllowNull),
                "Test_nullable_value_type__attr_JsonAllowNull__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonAllowNull {
	propDefault: number | null;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAllowNull__allow_override),
                "Test_nullable_value_type__attr_JsonAllowNull__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonAllowNull__allow_override {
	propDefault: number | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonAllowNull__allow_override),
                "Test_nullable_value_type__attr_JsonAllowNull__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonAllowNull__allow_override {
	propDefault: number | null;
	propOverriden: any | null;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDisallowNull),
                "Test_nullable_value_type__attr_JsonDisallowNull",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonDisallowNull {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDisallowNull),
                "Test_nullable_value_type__attr_JsonDisallowNull__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: false,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonDisallowNull {
	propDefault?: number;
	propOverriden: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDisallowNull__allow_override),
                "Test_nullable_value_type__attr_JsonDisallowNull__allow_override",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: false
                ),
                @"export interface Test_nullable_value_type__attr_JsonDisallowNull__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );

            yield return (
                typeof(Test_nullable_value_type__attr_JsonDisallowNull__allow_override),
                "Test_nullable_value_type__attr_JsonDisallowNull__allow_override__nulls_make_prop_optional",
                new TypeMappingConfig(
                    nullabilityHandling: NullabilityHandling.JsonProperty,
                    checkNullabilityForOverrides: true,
                    nullablePropertiesAreOptionalTooIfUnspecified: true
                ),
                @"export interface Test_nullable_value_type__attr_JsonDisallowNull__allow_override {
	propDefault?: number;
	propOverriden?: any;
}
"
            );
        }

        public class Test_reference_type__attr_None
        {
            public string PropDefault { get; set; }

            [TSSubstituteType("any")] public string PropOverriden { get; set; }
        }

        public class Test_reference_type__attr_None__allow_override
        {
            public string PropDefault { get; set; }

            [TSSubstituteType("any")] public string PropOverriden { get; set; }
        }


#nullable enable
        public class Test_nullable_reference_type__attr_None
        {
            public string? PropDefault { get; set; }

            [TSSubstituteType("any")] public string? PropOverriden { get; set; }
        }
#nullable restore


#nullable enable
        public class Test_nullable_reference_type__attr_None__allow_override
        {
            public string? PropDefault { get; set; }

            [TSSubstituteType("any")] public string? PropOverriden { get; set; }
        }
#nullable restore

        public class Test_value_type__attr_None
        {
            public int PropDefault { get; set; }

            [TSSubstituteType("any")] public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_None__allow_override
        {
            public int PropDefault { get; set; }

            [TSSubstituteType("any")] public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_RequiredAttr
        {
            [Required] public int PropDefault { get; set; }

            [TSSubstituteType("any")] [Required] public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_RequiredAttr__allow_override
        {
            [Required] public int PropDefault { get; set; }

            [TSSubstituteType("any")] [Required] public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_JsonDefault
        {
            [JsonProperty(Required = Required.Default)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_JsonDefault__allow_override
        {
            [JsonProperty(Required = Required.Default)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int PropOverriden { get; set; }
        }

        public class Test_value_type__attr_JsonAlways
        {
            [JsonProperty(Required = Required.Always)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Always)]
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

        public class Test_value_type__attr_JsonAllowNull
        {
            [JsonProperty(Required = Required.AllowNull)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.AllowNull)]
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

        public class Test_value_type__attr_JsonDisallowNull
        {
            [JsonProperty(Required = Required.DisallowNull)]
            public int PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.DisallowNull)]
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

        public class Test_nullable_value_type__attr_None
        {
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")] public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_None__allow_override
        {
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")] public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_RequiredAttr
        {
            [Required] public int? PropDefault { get; set; }

            [TSSubstituteType("any")] [Required] public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_RequiredAttr__allow_override
        {
            [Required] public int? PropDefault { get; set; }

            [TSSubstituteType("any")] [Required] public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_JsonDefault
        {
            [JsonProperty(Required = Required.Default)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_JsonDefault__allow_override
        {
            [JsonProperty(Required = Required.Default)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Default)]
            public int? PropOverriden { get; set; }
        }

        public class Test_nullable_value_type__attr_JsonAlways
        {
            [JsonProperty(Required = Required.Always)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.Always)]
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

        public class Test_nullable_value_type__attr_JsonAllowNull
        {
            [JsonProperty(Required = Required.AllowNull)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.AllowNull)]
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

        public class Test_nullable_value_type__attr_JsonDisallowNull
        {
            [JsonProperty(Required = Required.DisallowNull)]
            public int? PropDefault { get; set; }

            [TSSubstituteType("any")]
            [JsonProperty(Required = Required.DisallowNull)]
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