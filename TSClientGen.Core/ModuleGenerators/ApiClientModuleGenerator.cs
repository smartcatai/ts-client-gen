using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TSClientGen.ApiDescriptors;

namespace TSClientGen
{
	public class ApiClientModuleGenerator
	{
		public ApiClientModuleGenerator(
			ModuleDescriptor module,
			IPropertyNameProvider propertyNameProvider,
			Func<object, string> serializeToJson,
			string customCommonModuleName)
		{
			_module = module;
			_propertyNameProvider = propertyNameProvider;
			_serializeToJson = serializeToJson;
			_commonModuleName = customCommonModuleName ?? "./" + DefaultCommonModuleName;
		}
		
		
		public void WriteApiClient()
		{
			var imports = new List<string> { "request" };
			if (_module.Methods.Any(m => !m.UploadsFiles))
			{
				imports.Add("HttpRequestOptions");				
			}
			if (_module.Methods.Any(m => m.UploadsFiles))
			{
				imports.Add("UploadFileHttpRequestOptions");
				imports.Add("NamedBlob");
			}
			if (_module.Methods.Any(m => m.GenerateUrl))
			{
				imports.Add("getUri");
			}
			
			_result.AppendLine($"import {{ {string.Join(", ", imports)} }} from './{_commonModuleName}';");
			_result.AppendLine();
			
			_result.AppendLine($"export class {_module.ApiClientClassName} {{");
			
			if (_module.SupportsExternalHost)
			{
				_result.AppendLine("\tconstructor(private hostname?: string) {}");
				_result.AppendLine();
			}
			
			foreach (var method in _module.Methods)
			{
				if (method.GenerateUrl)
					generateMethod(method, true, _module.SupportsExternalHost, imports);

				generateMethod(method, false, _module.SupportsExternalHost, imports);
			}

			_result.AppendLine("}").AppendLine();

			_result.AppendLine($"export default new {_module.ApiClientClassName}();").AppendLine();
		}

		public void WriteType(CustomTypeDescriptor customType)
		{
			_result.AppendLine($"export type {_module.TypeMapping.GetTSType(customType.Type)} = {customType.TypeDefinition}").AppendLine();
		}
		
		public void WriteInterface(InterfaceDescriptor type)
		{
			_result.Append($"export interface {_module.TypeMapping.GetTSType(type.Type)} ");

			if (type.BaseType != null)
			{
				_result.Append($"extends {_module.TypeMapping.GetTSType(type.Type.BaseType)} ");
			}
			
			_result.AppendLine("{");

			if (!string.IsNullOrWhiteSpace(type.DiscriminatorFieldName))
			{
				_result.AppendLine($"\t{type.DiscriminatorFieldName}: {_module.TypeMapping.GetTSType(type.DiscriminatorFieldType)};");
			}
			
			foreach (var property in type.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				var name = _propertyNameProvider.GetPropertyName(property);
				if (name == null)
					continue;

				if (name == type.DiscriminatorFieldName)
				{
					throw new InvalidOperationException(
						$"Discriminator field name `{type.DiscriminatorFieldName}` can be equal property name `{type.Type.Name}::{name}`");
				}

				if (Nullable.GetUnderlyingType(property.PropertyType) != null)
					name += "?";

				_result.AppendLine($"\t{name}: {_module.TypeMapping.GetTSType(property)};");
			}

			_result.AppendLine("}").AppendLine();
		}
		
		public void WriteStaticContent(TSStaticContentAttribute staticContentModule)
		{
			foreach (var entry in staticContentModule.Content)
			{
				_result.AppendLine($"export let {entry.Key} = {_serializeToJson(entry.Value)};");
			}
		}
		
		public string GetResult()
		{
			return _result.ToString();
		}


		public static void WriteDefaultCommonModule(string outDir, HashSet<string> generatedFiles)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"TSClientGen." + DefaultCommonModuleName))
			using (var streamReader = new StreamReader(stream))
			{
				File.WriteAllText(Path.Combine(outDir, DefaultCommonModuleName), streamReader.ReadToEnd());
			}
			generatedFiles.Add(Path.Combine(outDir, DefaultCommonModuleName).ToLowerInvariant());			
		}

		
		private void generateMethod(
			MethodDescriptor method,
			bool generateGetUrl,
			bool supportsExternalHost,
			IEnumerable<string> moduleImports)
		{
			_result.Append(generateGetUrl 
				? $"\tpublic {toLowerCamelCase(method.Name)}Url("
				: $"\tpublic {toLowerCamelCase(method.Name)}(");

			var identifiersInUse = new HashSet<string>(
				moduleImports.Concat(
					new[] { "files", "cancelToken", "onUploadProgress", "url", "method", "params", "data", "blob" }));

			var nonFileParams = method.AllParams
				.Where(param => !method.UploadsFiles || !param.IsBodyContent)
				.ToList();
			foreach (var param in nonFileParams)
			{
				while (identifiersInUse.Contains(param.GeneratedName))
				{
					param.GeneratedName += "Param";
				}
				identifiersInUse.Add(param.GeneratedName);
			}
			
			var parameters = nonFileParams
				.Where(param => !param.IsOptional)
				.Select(param => $"{param.GeneratedName}: {param.Type}")
				.ToList();
			if (!generateGetUrl)
			{
				if (method.UploadsFiles)
				{
					parameters.Add("files: Array<NamedBlob | File>");
					parameters.Add("{ cancelToken, onUploadProgress }: UploadFileHttpRequestOptions = {}");
				}
				else
				{
					parameters.Add("{ cancelToken }: HttpRequestOptions = {}");					
				}
			}
			parameters.AddRange(nonFileParams
				.Where(param => param.IsOptional)
				.Select(param => $"{param.GeneratedName}?: {param.Type}"));

			_result.Append(string.Join(", ", parameters)).AppendLine(") {");

			generateMethodBody(method, generateGetUrl, supportsExternalHost);
			_result.AppendLine("\t}").AppendLine();
		}

		private void generateMethodBody(
			MethodDescriptor methodDescriptor, 
			bool generateGetUrl,
			bool supportsExternalHost)
		{
			_result.AppendLine($"\t\tconst method = '{methodDescriptor.HttpVerb.ToLower()}';");
		
			string url = methodDescriptor.UrlTemplate;
			foreach (var param in methodDescriptor.UrlParamsByPlaceholder)
			{
				string paramValue = param.Value.GeneratedName;
				if (param.Value.Type == "Date")
				{
					paramValue += ".toISOString()";
				}
				url = url.Replace(param.Key,"${" + paramValue + "}");
			}
			
			_result.AppendLine(supportsExternalHost
				? $"\t\tconst url = (this.hostname || '') + `{url}`;"
				: $"\t\tconst url = `{url}`;");

			var requestParams = new List<string> { "url", "method" };
			
			if (methodDescriptor.QueryParams.Any())
			{
				requestParams.Add("params");
				var queryParams = methodDescriptor.QueryParams.Select(p =>
					(p.OriginalName == p.GeneratedName)
						? p.OriginalName
						: $"{p.OriginalName}: {p.GeneratedName}"); 
				_result.AppendLine($"\t\tconst params = {{ {string.Join(", ", queryParams)} }};");
			}

			if (generateGetUrl)
			{
				_result.AppendLine($"\t\treturn getUri({{ {string.Join(", ", requestParams)} }});");
				return;
			}
			
			if (methodDescriptor.UploadsFiles)
			{
				requestParams.Add("data");
				_result.AppendLine("\t\tconst data = new FormData();");
				_result.AppendLine("\t\tfor (const f of files) {");
				_result.AppendLine("\t\t\tconst namedBlob = f as NamedBlob;");
				_result.AppendLine("\t\t\tif (namedBlob.blob && namedBlob.name) {");
				_result.AppendLine("\t\t\t\tdata.append('file', namedBlob.blob, namedBlob.name);");
				_result.AppendLine("\t\t\t} else {");
				_result.AppendLine("\t\t\t\tdata.append('file', f as File);");
				_result.AppendLine("\t\t\t}");
				_result.AppendLine("\t\t}");

				if (methodDescriptor.BodyParam.IsModelWithFiles)
				{
					_result.AppendLine($"\t\tconst blob = new Blob([JSON.stringify({methodDescriptor.BodyParam.GeneratedName})], {{ type: 'application/json' }});");
					_result.AppendLine("\t\tdata.append('Value', blob);");
				}
			}
			else if (methodDescriptor.BodyParam != null)
			{
				requestParams.Add("data");
				_result.AppendLine($"\t\tconst data = {methodDescriptor.BodyParam.GeneratedName};");
			}

			requestParams.AddRange(new[] { "cancelToken" });
			if (methodDescriptor.UploadsFiles)
			{
				requestParams.Add("onUploadProgress");
			}
			
			_result.AppendLine($"\t\treturn request<{methodDescriptor.ReturnType}>({{ {string.Join(", ", requestParams)} }});");
		}		

		private string toLowerCamelCase(string name)
		{
			return char.ToLowerInvariant(name[0]) + name.Substring(1);
		}
		
		
		private const string DefaultCommonModuleName = "common.ts";		
		
		private readonly ModuleDescriptor _module;
		private readonly IPropertyNameProvider _propertyNameProvider;
		private readonly Func<object, string> _serializeToJson;
		private readonly string _commonModuleName;
		private readonly StringBuilder _result = new StringBuilder();
	}
}