using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

#nullable enable

namespace TSClientGen.Tests.CodeGen
{
	[Generator]
	public class NullabilityIntegrationTestCasesGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			// no initialization needed
		}
		
		private const string _className = "NullabilityIntegrationTestCases";

		public void Execute(GeneratorExecutionContext context)
		{
			//context.Compilation.Assembly.
			var source = generateCode();
			context.AddSource(_className, source);
		}

		private static readonly bool[] _flags = new[] {false, true};
		private static readonly string _propertyNameNonOverriden = "PropDefault";
		private static readonly string _propertyNameOverriden = "PropOverriden";
		private static readonly string _valType = "int";
		private static readonly string _valTypeTs = "number";
		private static readonly string _refType = "string";
		private static readonly string _refTypeTs = "string";

		private string generateCode()
		{
			var typeDefinitions = new List<string>();
			var sb = new StringBuilder();
			sb.Append(@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using TSClientGen;

namespace TSClientGen.Tests.CodeGen
{").AppendLine()
				.Append("public static class ").Append(_className).AppendLine()
				.Append("{").AppendLine()
				.Append("public static IEnumerable<(Type testType, string caseName, TypeMappingConfig config, string expectation)> EnumerateCases()")
				.AppendLine()
				.Append("{").AppendLine();

			var parameters =
				from isValueType in _flags
				from isNullableType in _flags
				from attr in PropertyAttr.All
				from allowOverrideOverriding in _flags
				let valueTypeNullable = isValueType && isNullableType
				let useNullableReferenceTypes = !isValueType && isNullableType && attr is PropertyAttr.None
				let isNullable = valueTypeNullable || useNullableReferenceTypes
				where isNullable || isValueType || attr is PropertyAttr.None
				select (isValueType, isNullable, useNullableReferenceTypes, attr, allowOverrideOverriding);
			foreach (var (isValueType, isNullable, useNrt, attr, allowOverrideOverriding) in parameters)
			{
				var type =
					generateTestType(isValueType, isNullable, useNrt, attr, allowOverrideOverriding);
				typeDefinitions.Add(type.typeDefinition);
				foreach (var nullablePropsAreOptionalToo in _flags)
				{
					var expectation = generateExpectation(type.typeName, isValueType, isNullable, attr,
						allowOverrideOverriding, nullablePropsAreOptionalToo);
					var configCtorCall = generateConfigCtorCall(attr, useNrt, allowOverrideOverriding,
						nullablePropsAreOptionalToo);
					var caseName = nullablePropsAreOptionalToo
						? type.typeName + "__nulls_make_prop_optional"
						: type.typeName;
					sb
						.Append("yield return (")
						.AppendLine()
						.Append("typeof(").Append(type.typeName).Append("),").AppendLine()
						.Append('"').Append(caseName).Append("\",").AppendLine()
						.Append(configCtorCall).Append(",").AppendLine()
						.Append(expectation).AppendLine()
						.Append(");").AppendLine().AppendLine();
				}
			}

			sb.AppendLine().Append("}").AppendLine().AppendLine();
			foreach (var definition in typeDefinitions)
				sb.Append(definition);
			sb.AppendLine().Append("}")
				.AppendLine().Append("}");
			return sb.ToString();

			string generateConfigCtorCall(PropertyAttr attr, bool useNullReferenceTypes, bool allowOverrideOverriding,
				bool nullablePropsAreOptionalTooIfUnspecifiedExplicitly)
			{
				var nullHandling =
					useNullReferenceTypes
						? "Nrt"
						: attr switch
						{
							PropertyAttr.None _ => "Default",
							PropertyAttr.RequiredAttr _ => "DataAnnotations",
							_ => "JsonProperty"
						};
				var sb = new StringBuilder();
				sb.Append("new TypeMappingConfig(").AppendLine()
					.Append("nullabilityHandling: NullabilityHandling.").Append(nullHandling).Append(",").AppendLine()
					.Append("checkNullabilityForOverrides: ").Append(allowOverrideOverriding.ToString().ToLowerInvariant()).Append(",").AppendLine()
					.Append("nullablePropertiesAreOptionalTooIfUnspecified: ").Append(nullablePropsAreOptionalTooIfUnspecifiedExplicitly.ToString().ToLowerInvariant()).AppendLine()
					.Append(")");
				return sb.ToString();
			}

			string generateExpectation(string typeName, bool isValueType, bool isNullable, PropertyAttr attr,
				bool allowOverrideOverriding, bool nullablePropsAreOptionalTooIfUnspecifiedExplicitly)
			{
				var type = (isValueType ? _valTypeTs : _refTypeTs)!;
				var (isTsNullable, isOptional) =
					attr switch
					{
						PropertyAttr.None _ => (
							isNullable && !isValueType,
							isNullable && (isValueType || nullablePropsAreOptionalTooIfUnspecifiedExplicitly)),
						PropertyAttr.RequiredAttr _ => (
							false,
							isNullable),
						PropertyAttr.JsonAlways _ => (false, false),
						PropertyAttr.JsonDefault _ => (true, true),
						PropertyAttr.JsonAllowNull _ => (true, false),
						PropertyAttr.JsonDisallowNull _ => (false, true)
					};
				var sb = new StringBuilder();
				sb.Append("@\"export interface ").Append(typeName).AppendLine(" {");
				appendProperty(_propertyNameNonOverriden!, type, isTsNullable, isOptional);
				appendProperty(_propertyNameOverriden!, "any",
					allowOverrideOverriding && isTsNullable,
					allowOverrideOverriding && isOptional);
				sb.AppendLine("}").Append('"');

				return sb.ToString();

				void appendProperty(string propertyName, string type, bool isNullable, bool isOptional)
				{
					sb!.Append("\t").Append(char.ToLowerInvariant(propertyName[0]));
					sb.Append(propertyName, 1, propertyName.Length - 1);
					if (isOptional) sb.Append("?");
					sb.Append(": ").Append(type);
					if (isNullable) sb.Append(" | null");
					sb.AppendLine(";");
				}
			}

			(string typeName, string typeDefinition) generateTestType(bool isValueType, bool isNullable,
				bool useNullableReferenceTypes, PropertyAttr attr, bool allowOverrideOverriding)
			{
				var typeNameBuilder = new StringBuilder("Test_");
				typeNameBuilder
					.Append(isNullable ? "nullable_" : "")
					.Append(isValueType ? "value" : "reference")
					.Append("_type__attr_")
					.Append(attr);
				if (allowOverrideOverriding)
					typeNameBuilder.Append("__allow_override");

				var typeName = typeNameBuilder.ToString();

				var definitionBuilder = new StringBuilder();
				if (useNullableReferenceTypes)
					definitionBuilder.AppendLine().Append("#nullable enable").AppendLine();

				definitionBuilder
					.Append("public class ").Append(typeName).AppendLine()
					.Append("{").AppendLine();

				appendProperty(_propertyNameNonOverriden)
					.AppendLine()
					.Append(@"[TSSubstituteType(""any"")]")
					.AppendLine();

				appendProperty(_propertyNameOverriden);

				definitionBuilder
					.AppendLine().Append("}");

				if (useNullableReferenceTypes)
					definitionBuilder.AppendLine().Append("#nullable restore");

				definitionBuilder
					.AppendLine()
					.AppendLine();;

				var definition = definitionBuilder.ToString();

				return (typeName, definition);

				StringBuilder appendProperty(string propertyName)
				{
					var sb =
						attr switch
						{
							PropertyAttr.None _ => definitionBuilder,
							PropertyAttr.RequiredAttr _ => definitionBuilder.Append("[Required]").AppendLine(),
							PropertyAttr.JsonAlways _ => definitionBuilder
								.Append("[JsonProperty(Required = Required.Always)]").AppendLine(),
							PropertyAttr.JsonDefault _ => definitionBuilder
								.Append("[JsonProperty(Required = Required.Default)]").AppendLine(),
							PropertyAttr.JsonAllowNull _ => definitionBuilder
								.Append("[JsonProperty(Required = Required.AllowNull)]").AppendLine(),
							PropertyAttr.JsonDisallowNull _ => definitionBuilder
								.Append("[JsonProperty(Required = Required.DisallowNull)]").AppendLine()
						};

					sb.Append("public ").Append(isValueType ? _valType : _refType);
					if (isNullable && (isValueType || useNullableReferenceTypes))
						sb.Append("?");
					sb
						.Append(" ").Append(propertyName).Append(" { get; set; }");

					return sb;
				}
			}
		}
	}
}