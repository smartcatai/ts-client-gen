using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;

namespace TSClientGen
{
	static class TSGenerator
	{
		public static void GenerateControllerClient(this StringBuilder result, Type controller, TypeMapper mapper, RouteAttribute controllerRoute)
		{
			var routePrefix = controller.GetCustomAttributes<RoutePrefixAttribute>().SingleOrDefault()?.Prefix;
			if (controllerRoute != null && routePrefix != null)
				throw new Exception("Controller has Route and RoutePrefix attributes at the same time");

			routePrefix = string.IsNullOrEmpty(routePrefix) ? "/" : "/" + routePrefix + "/";

			result.AppendLine($"export class {controller.Name.Replace("Controller", "")}Client {{");

			var actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToArray();
			foreach (var action in actions)
			{
				var descriptor = ActionDescriptor.TryCreateFrom(action, controllerRoute, routePrefix);
				if (descriptor == null)
					continue;

				if (descriptor.GenerateUrl)
					generateMethod(result, mapper, action, descriptor, true, false);

				generateMethod(result, mapper, action, descriptor, false, descriptor.GenerateUploadProgressCallback);
			}

			result.AppendLine("}");
			result.AppendLine();
		}

		public static void GenerateStaticContent(this StringBuilder result, TSStaticContentAttribute staticContentModule)
		{
			foreach (var entry in staticContentModule.Content)
			{
				result.AppendLine($"export let {entry.Key} = {JsonConvert.SerializeObject(entry.Value)};");
			}
		}

		public static void GenerateEnums(
			this StringBuilder sb,
			IEnumerable<Type> enumTypes,
			ILookup<Type, TSExtendEnumAttribute> staticMemberProvidersByEnum,
			TypeMapper mapper)
		{
			var requireResourceImport = false;

			foreach (var @enum in enumTypes)
			{
				sb.GenerateEnum(@enum, mapper);

				if (!staticMemberProvidersByEnum[@enum].Any())
					continue;

				sb.AppendLine($"export namespace {@enum.Name} {{");

				foreach (var provider in staticMemberProvidersByEnum[@enum])
				{
					provider.GenerateStaticMembers(sb);
				}

				sb.AppendLine("}");

				requireResourceImport = true;
			}

			if (requireResourceImport)
			{
				sb.AppendLine();
				sb.AppendLine("function getResource(key: string) {");
				sb.AppendLine("	let locale = (<any>window).locale;");
				sb.AppendLine("	let value = resx.messages[locale][key] || resx.messages['ru'][key];");
				sb.AppendLine("	if (!value) console.warn('Key ' + key + ' has not been found in enums.resx');");
				sb.AppendLine("	return value || key;");
				sb.AppendLine("}");
				sb.AppendLine("import resx from '../enums.resx'");
			}
		}

		public static void GenerateEnum(this StringBuilder sb, Type enumType, TypeMapper mapper)
		{
			var names = Enum.GetNames(enumType);
			var underlyingType = Enum.GetUnderlyingType(enumType);

			sb.AppendLine($"export enum {mapper.GetTSType(enumType)} {{");

			foreach (string name in names)
			{
				var value = Convert.ChangeType(Enum.Parse(enumType, name), underlyingType);
				sb.AppendLine($"\t{name} = {value},");
			}

			sb.AppendLine("}");
			sb.AppendLine();
		}

		public static void GenerateEnumLocalizations(this ResXResourceWriter resxWriter, IReadOnlyCollection<TSEnumLocalizationAttribute> enumLocalizations)
		{
			foreach (var enumLocalization in enumLocalizations)
			{
				generateEnumResxEntries(resxWriter, enumLocalization);
				if (enumLocalization.AdditionalContexts != null)
				{
					foreach (var context in enumLocalization.AdditionalContexts)
					{
						generateEnumResxEntries(resxWriter, enumLocalization, context);
					}
				}
			}
		}

		private static void generateEnumResxEntries(ResXResourceWriter resxWriter, TSEnumLocalizationAttribute enumLocalization, string context = null)
		{
			var enumName = enumLocalization.EnumType.Name;
			foreach (var valueName in Enum.GetNames(enumLocalization.EnumType))
			{
				string valueNameWithContext = (context != null) ? $"{context}_{valueName}" : valueName;
				string resourceKey = enumLocalization.UsePrefix ? $"{enumName}_{valueNameWithContext}" : valueNameWithContext;
				var localization = enumLocalization.ResourceManager.GetString(resourceKey);
				if (localization == null && context != null)
				{
					resourceKey = enumLocalization.UsePrefix ? $"{enumName}_{valueName}" : valueName;
					localization = enumLocalization.ResourceManager.GetString(resourceKey);
				}
				if (localization == null)
				{
					throw new Exception(
						$"Enum value {enumName}.{valueName} is not localized in RESX {enumLocalization.ResxName} (context - {context ?? "none"}, key - {resourceKey})");
				}

				resxWriter.AddResource($"{enumName}_{valueNameWithContext}", localization);
			}
		}

		public static void GenerateInterface(this StringBuilder sb, CustomTypeDescriptor modelType, TypeMapper mapper)
		{
			sb.Append($"export interface {mapper.GetTSType(modelType.Type)} ");

			if (modelType.BaseType != null)
			{
				sb.Append($"extends {mapper.GetTSType(modelType.Type.BaseType)} ");
			}
			
			sb.AppendLine("{");

			if (!string.IsNullOrWhiteSpace(modelType.DiscriminatorFieldName))
			{
				sb.AppendLine($"\t{modelType.DiscriminatorFieldName}: {mapper.GetTSType(modelType.DiscriminatorFieldType)};");
			}
			
			foreach (var property in modelType.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				var ignoreDataMember = property.GetCustomAttributes<IgnoreDataMemberAttribute>().FirstOrDefault();
				if (ignoreDataMember != null)
					continue;

				var dataMember = property.GetCustomAttributes<DataMemberAttribute>().FirstOrDefault();
				var jsonProperty = property.GetCustomAttributes<JsonPropertyAttribute>().FirstOrDefault();
				var name = dataMember != null
					? dataMember.Name
					: jsonProperty != null
						? jsonProperty.PropertyName
						: property.Name.toLowerCamelCase();

				if (name == modelType.DiscriminatorFieldName)
					throw new InvalidOperationException(
						$"Discriminator field name `{modelType.DiscriminatorFieldName}` can be equal property name `{modelType.Type.Name}::{name}`");
				
				if (Nullable.GetUnderlyingType(property.PropertyType) != null)
					name += "?";

				sb.AppendLine($"\t{name}: {mapper.GetTSType(property)};");
			}

			sb.AppendLine("}");
			sb.AppendLine("");
		}

		public static void GenerateTypeDefinition(this StringBuilder sb, CustomTypeDescriptor modelType, TypeMapper mapper)
		{
			sb.AppendLine($"export type {mapper.GetTSType(modelType.Type)} = {modelType.TypeDefinition}");
			sb.AppendLine("");
		}

		private static void generateMethod(
			StringBuilder result,
			TypeMapper mapper,
			MethodInfo action,
			ActionDescriptor descriptor,
			bool generateGetUrl,
			bool generateProgressCallback)
		{
			result.Append(generateGetUrl 
				? $"\t{action.Name.toLowerCamelCase()}Url("
				: $"\t{action.Name.toLowerCamelCase()}(");

			var parameters = descriptor.AllParams
				.Where(param => !descriptor.IsUploadedFile || param != descriptor.BodyParam)
				.Select(param => $"{param.Name}{(param.IsOptional ? "?" : "")}: {mapper.GetTSType(param.ParameterType)}")
				.ToList();
			if (descriptor.IsModelWithFiles || descriptor.IsUploadedFile)
				parameters.Add("files: File[]");

			if (generateProgressCallback)
				parameters.Add("progressCallback?: (event: ProgressEvent) => void");

			if (!generateGetUrl)
				parameters.Add("suppressAjaxError?: boolean");

			result.Append(string.Join(", ", parameters));
			result.AppendLine(generateGetUrl
				? $"): string {{"
				: $"): Promise<{mapper.GetTSType(action.ReturnType)}> {{");

			generateMethodBody(result, descriptor, action, generateGetUrl, generateProgressCallback);
			result.AppendLine("\t}");
			result.AppendLine();
		}

		private static void generateMethodBody(
			this StringBuilder result, 
			ActionDescriptor actionDescriptor, 
			MethodInfo action,
			bool generateGetUrl,
			bool generateProgressCallback)
		{
			result.AppendLine($"\t\tvar url = '{actionDescriptor.RouteTemplate}';");

			foreach (var param in actionDescriptor.RouteParamsBySections)
			{
				var paramPreprocessCallString = param.Value.ParameterType == typeof(DateTime)
					? "toISOString()"
					: "toString()";
				result.AppendLine($"\t\turl = url.replace('{param.Key}', {param.Value.Name}.{paramPreprocessCallString});");
			}

			if (actionDescriptor.QueryParams.Any())
			{
				result.AppendLine("\t\tvar queryParams: any = {};");

				foreach (var param in actionDescriptor.QueryParams)
				{
					result.AppendLine($"\t\tif (typeof {param.Name} != 'undefined') {{");
					result.AppendLine($"\t\t\tqueryParams.{param.Name} = {param.Name};");
					result.AppendLine($"\t\t}}");
				}

				result.AppendLine($"\t\tvar queryString = $.param(queryParams);");
				result.AppendLine($"\t\tif (queryString) {{");
				result.AppendLine("\t\t\turl = url + '?' + queryString;");
				result.AppendLine("\t\t}");
			}

			if (generateGetUrl)
			{
				result.AppendLine("\t\treturn url;");
				return;
			}

			if (actionDescriptor.IsModelWithFiles || actionDescriptor.IsUploadedFile)
			{
				result.AppendLine("\t\tvar formData = new FormData();");
				result.AppendLine("\t\tfor (let f of files) {");
				result.AppendLine("\t\t\tformData.append('file', f);");
				result.AppendLine("\t\t}");

				if (actionDescriptor.IsModelWithFiles)
				{
					result.AppendLine($"\t\tvar blob = new Blob([JSON.stringify({actionDescriptor.BodyParam.Name})], {{ type: 'application/json' }});");
					result.AppendLine("\t\tformData.append('Value', blob);");
				}
			}

			result.AppendLine("\t\treturn ajax({");
			result.AppendLine($"\t\t\turl: url,");
			result.AppendLine("\t\t\terror: (jqXhr: JQueryXHR) => { jqXhr.suppressAjaxError = suppressAjaxError; },");

			if (generateProgressCallback)
			{
				result.AppendLine("\t\t\txhr: function () {");
				result.AppendLine("\t\t\t\tvar xhr = new XMLHttpRequest();");
				result.AppendLine("\t\t\t\txhr.upload.onprogress = progressCallback;");
				result.AppendLine("\t\t\t\treturn xhr;");
				result.AppendLine("\t\t\t},");
			}

			result.AppendLine($"\t\t\ttype: '{actionDescriptor.HttpVerb}',");
			if (actionDescriptor.BodyParam != null)
			{
				if (actionDescriptor.IsModelWithFiles || actionDescriptor.IsUploadedFile)
				{
					if (action.ReturnType != typeof(void))
					{
						result.AppendLine("\t\t\tparseResponseAsJson: true,");
					}
					result.AppendLine("\t\t\tcontentType: false,");
					result.AppendLine("\t\t\tprocessData: false,");
					result.AppendLine($"\t\t\tdata: formData");
				}
				else
				{
					result.AppendLine("\t\t\tcontentType: 'application/json',");
					result.AppendLine($"\t\t\tdata: JSON.stringify({actionDescriptor.BodyParam.Name})");
				}
				
			}
			result.AppendLine("\t\t});");
		}

		private static string toLowerCamelCase(this string name)
		{
			return char.ToLowerInvariant(name[0]) + name.Substring(1);
		}
	}
}