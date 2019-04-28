using System;
using System.Collections.Generic;
using System.Linq;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class ApiModuleGenerator
	{
		public ApiModuleGenerator(
			ApiClientModule apiClientModule,
			TypeMapping typeMapping,
			Func<object, string> serializeToJson,
			string commonModuleName)
		{
			_apiClientModule = apiClientModule;
			_typeMapping = typeMapping;
			_serializeToJson = serializeToJson;
			_commonModuleName = commonModuleName;
		}
		
		
		public void WriteApiClientClass()
		{
			var imports = new List<string> { "request" };
			if (_apiClientModule.Methods.Any(m => !m.UploadsFiles))
			{
				imports.Add("HttpRequestOptions");				
			}
			if (_apiClientModule.Methods.Any(m => m.UploadsFiles))
			{
				imports.Add("UploadFileHttpRequestOptions");
				imports.Add("NamedBlob");
			}
			if (_apiClientModule.Methods.Any(m => m.GenerateUrl))
			{
				imports.Add("getUri");
			}
			
			_result
				.AppendLine($"import {{ {string.Join(", ", imports)} }} from '{_commonModuleName}';")
				.AppendLine()
				.AppendLine($"export class {_apiClientModule.ApiClientClassName} {{")
				.Indent();
			
			if (_apiClientModule.SupportsExternalHost)
			{
				_result.AppendLine("constructor(private hostname?: string) {}").AppendLine();
			}
			
			foreach (var method in _apiClientModule.Methods)
			{
				var methodWriter = new ApiMethodGenerator(method, _result, _typeMapping);
				methodWriter.ResolveConflictingParamNames(imports);
				if (method.GenerateUrl)
				{					
					writeMethod(
						() => methodWriter.WriteGetUrlSignature(), 
						() => methodWriter.WriteBody(true, _apiClientModule.SupportsExternalHost));
				}

				writeMethod(
					() => methodWriter.WriteSignature(),
					() => methodWriter.WriteBody(false, _apiClientModule.SupportsExternalHost));
			}

			_result
				.Unindent()
				.AppendLine("}").AppendLine()
				.AppendLine($"export default new {_apiClientModule.ApiClientClassName}();").AppendLine();
		}

		public void WriteTypeDefinitions()
		{
			foreach (var type in _typeMapping.GetGeneratedTypes())
			{
				_result.AppendText(type.Value).AppendLine();
			}
		}

		public void WriteEnumImports(string enumsModuleName)
		{
			var enumTypes = _typeMapping.GetEnums();
			if (enumTypes.Any())
			{
				_result.Append("import { ")
					.Append(string.Join(", ", enumTypes.Select(t => t.Name)))
					.AppendLine($" }} from './{enumsModuleName}'")
					.AppendLine();
			}
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
		
		
		private void writeMethod(Action writeSignature, Action writeBody)
		{
			writeSignature();
			_result.AppendLine(" {").Indent();
			writeBody();
			_result.Unindent().AppendLine("}").AppendLine();
		}		
		
		private readonly ApiClientModule _apiClientModule;
		private readonly TypeMapping _typeMapping;
		private readonly Func<object, string> _serializeToJson;
		private readonly string _commonModuleName;
		private readonly IndentedStringBuilder _result = new IndentedStringBuilder();
	}
}