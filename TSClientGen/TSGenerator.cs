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
			
			var actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToArray();
			var actionDescriptors = actions.Select(a => ActionDescriptor.TryCreateFrom(a, controllerRoute, routePrefix, mapper)).ToList();

			var imports = new List<string> { "request" };
			if (actionDescriptors.Any(a => a != null && !a.IsUploadedFile && !a.IsModelWithFiles))
			{
				imports.Add("HttpRequestOptions");				
			}
			if (actionDescriptors.Any(a => a != null && (a.IsUploadedFile || a.IsModelWithFiles)))
			{
				imports.Add("UploadFileHttpRequestOptions");
				imports.Add("NamedBlob");
			}
			if (actionDescriptors.Any(a => a != null && a.GenerateUrl))
			{
				imports.Add("getUri");
			}
			
			result.AppendLine($"import {{ {string.Join(", ", imports)} }} from './common';");
			result.AppendLine();
			
			string apiClientClassName = controller.Name.Replace("Controller", "Client"); 
			result.AppendLine($"export class {apiClientClassName} {{");
			
			var supportsExternalHost = controller.GetCustomAttributes<TSSupportsExternalHostAttribute>().Any();
			if (supportsExternalHost)
			{
				result.AppendLine("\tconstructor(private hostname?: string) {}");
				result.AppendLine();
			}
			
			foreach (var action in actions
				.Zip(actionDescriptors, (a, d) => new { MethodInfo = a, Descriptor = d})
				.Where(a => a.Descriptor != null))
			{
				if (action.Descriptor.GenerateUrl)
					generateMethod(result, action.Descriptor, true, supportsExternalHost, imports);

				generateMethod(result, action.Descriptor, false, supportsExternalHost, imports);
			}

			result.AppendLine("}");
			result.AppendLine();

			result.AppendLine($"export default new {apiClientClassName}();");
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
			TypeMapper mapper,
			string defaultLocale)
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

					if (provider is TSEnumLocalizationAttribute)
					{
						requireResourceImport = true;
					}
				}

				sb.AppendLine("}");
			}

			if (requireResourceImport)
			{
				sb.AppendLine();
				sb.AppendLine("function getResource(key: string) {");
				sb.AppendLine("\tlet locale = (<any>window).locale;");
				sb.AppendLine($"\tlet value = resx.messages[locale][key] || resx.messages['{defaultLocale}'][key];");
				sb.AppendLine("\tif (!value) console.warn('Key ' + key + ' has not been found in enums.resx');");
				sb.AppendLine("\treturn value || key;");
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
				{
					throw new InvalidOperationException(
						$"Discriminator field name `{modelType.DiscriminatorFieldName}` can be equal property name `{modelType.Type.Name}::{name}`");
				}

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
			ActionDescriptor descriptor,
			bool generateGetUrl,
			bool supportsExternalHost,
			IEnumerable<string> moduleImports)
		{
			result.Append(generateGetUrl 
				? $"\tpublic {descriptor.Name.toLowerCamelCase()}Url("
				: $"\tpublic {descriptor.Name.toLowerCamelCase()}(");

			var identifiersInUse = new HashSet<string>(
				moduleImports.Concat(
					new[] { "files", "cancelToken", "onUploadProgress", "url", "method", "params", "data", "blob" }));
			
			var parameters = descriptor.AllParams
				.Where(param => !descriptor.IsUploadedFile || !param.IsBodyContent)
				.Select(param =>
				{
					while (identifiersInUse.Contains(param.TypescriptAlias))
					{
						param.TypescriptAlias += "Param";
					}
					return $"{param.TypescriptAlias}{(param.IsOptional ? "?" : "")}: {param.TypescriptType}";
				})
				.ToList();
			if (!generateGetUrl)
			{
				if (descriptor.IsModelWithFiles || descriptor.IsUploadedFile)
				{
					parameters.Add("files: Array<NamedBlob | File>");
					parameters.Add("{ cancelToken, onUploadProgress }: UploadFileHttpRequestOptions = {}");
				}
				else
				{
					parameters.Add("{ cancelToken }: HttpRequestOptions = {}");					
				}
			}

			result.Append(string.Join(", ", parameters));
			result.AppendLine(") {");

			generateMethodBody(result, descriptor, generateGetUrl, supportsExternalHost);
			result.AppendLine("\t}");
			result.AppendLine();
		}

		private static void generateMethodBody(
			this StringBuilder result, 
			ActionDescriptor actionDescriptor, 
			bool generateGetUrl,
			bool supportsExternalHost)
		{
			result.AppendLine($"\t\tconst method = '{actionDescriptor.HttpVerb.ToLower()}';");
		
			string url = actionDescriptor.RouteTemplate;
			foreach (var param in actionDescriptor.RouteParamsBySections)
			{
				string paramValue = param.Value.TypescriptAlias;
				if (param.Value.DotNetType == typeof(DateTime))
				{
					paramValue += ".toISOString()";
				}
				url = url.Replace(param.Key,"${" + paramValue + "}");
			}
			
			result.AppendLine(supportsExternalHost
				? $"\t\tconst url = (this.hostname || '') + `{url}`;"
				: $"\t\tconst url = `{url}`;");

			var requestParams = new List<string> { "url", "method" };
			
			if (actionDescriptor.QueryParams.Any())
			{
				requestParams.Add("params");
				var queryParams = actionDescriptor.QueryParams.Select(p =>
					(p.Name == p.TypescriptAlias)
						? p.Name
						: $"{p.Name}: {p.TypescriptAlias}"); 
				result.AppendLine($"\t\tconst params = {{ {string.Join(", ", queryParams)} }};");
			}

			if (generateGetUrl)
			{
				result.AppendLine($"\t\treturn getUri({{ {string.Join(", ", requestParams)} }});");
				return;
			}
			
			if (actionDescriptor.IsModelWithFiles || actionDescriptor.IsUploadedFile)
			{
				requestParams.Add("data");
				result.AppendLine("\t\tconst data = new FormData();");
				result.AppendLine("\t\tfor (const f of files) {");
				result.AppendLine("\t\t\tconst namedBlob = f as NamedBlob;");
				result.AppendLine("\t\t\tif (namedBlob.blob && namedBlob.name) {");
				result.AppendLine("\t\t\t\tdata.append('file', namedBlob.blob, namedBlob.name);");
				result.AppendLine("\t\t\t} else {");
				result.AppendLine("\t\t\t\tdata.append('file', f as File);");
				result.AppendLine("\t\t\t}");
				result.AppendLine("\t\t}");

				if (actionDescriptor.IsModelWithFiles)
				{
					result.AppendLine($"\t\tconst blob = new Blob([JSON.stringify({actionDescriptor.BodyParam.TypescriptAlias})], {{ type: 'application/json' }});");
					result.AppendLine("\t\tdata.append('Value', blob);");
				}
			}
			else if (actionDescriptor.BodyParam != null)
			{
				requestParams.Add("data");
				result.AppendLine($"\t\tconst data = {actionDescriptor.BodyParam.TypescriptAlias};");
			}

			requestParams.AddRange(new[] { "cancelToken" });
			if (actionDescriptor.IsModelWithFiles || actionDescriptor.IsUploadedFile)
			{
				requestParams.Add("onUploadProgress");
			}
			
			result.AppendLine($"\t\treturn request<{actionDescriptor.ReturnType}>({{ {string.Join(", ", requestParams)} }});");
		}

		private static string toLowerCamelCase(this string name)
		{
			return char.ToLowerInvariant(name[0]) + name.Substring(1);
		}
	}
}