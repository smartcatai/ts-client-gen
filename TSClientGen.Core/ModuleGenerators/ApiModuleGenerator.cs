using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class ApiModuleGenerator
	{
		public ApiModuleGenerator(
			ApiClientModule apiClientModule,
			TypeMapping typeMapping,
			IApiClientWriter customApiClientWriter,
			Func<object, string> serializeToJson,
			string transportModuleName)
		{
			_apiClientModule = apiClientModule;
			_typeMapping = typeMapping;
			_customApiClientWriter = customApiClientWriter;
			_serializeToJson = serializeToJson;
			_transportModuleName = transportModuleName;
		}
		
		
		public void WriteApiClientClass()
		{
			var transportContractInterfaces = new List<string>();
			if (_apiClientModule.Methods.Any(m => !m.UploadsFiles))
			{
				transportContractInterfaces.Add("HttpRequestOptions");
			}
			if (_apiClientModule.Methods.Any(m => m.UploadsFiles))
			{
				transportContractInterfaces.Add("UploadFileHttpRequestOptions");
				transportContractInterfaces.Add("NamedBlob");
			}

			var transportImports = _apiClientModule.Methods.Any(m => m.GenerateUrl)
				? new[] { "request", "getUri" }
				: new[] { "request" };
			
			_result
				.AppendLine($"import {{ {string.Join(", ", transportContractInterfaces)} }} from './{TransportContractsModuleName}';")
				.AppendLine($"import {{ {string.Join(", ", transportImports)} }} from '{_transportModuleName}';")
				.AppendLine();

			_customApiClientWriter?.WriteImports(_result, _apiClientModule);
			_result.AppendLine();
			_customApiClientWriter?.WriteCodeBeforeApiClientClass(_result, _apiClientModule);

			_result
				.AppendLine($"export class {_apiClientModule.ApiClientClassName} {{")
				.Indent();

			_result.Append("constructor(");
			if (_apiClientModule.SupportsExternalHost)
			{
				_result.Append("private hostname?: string");
			}

			_result.AppendLine(") {");
			_customApiClientWriter?.ExtendApiClientConstructor(_result, _apiClientModule);
			_result.AppendLine("}");
			
			foreach (var method in _apiClientModule.Methods)
			{
				var methodWriter = new ApiMethodGenerator(method, _result, _typeMapping);
				methodWriter.ResolveConflictingParamNames(transportImports);
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
			
			_customApiClientWriter?.ExtendApiClientClass(_result, _apiClientModule);

			_result
				.Unindent()
				.AppendLine("}").AppendLine()
				.AppendLine($"export default new {_apiClientModule.ApiClientClassName}();").AppendLine();

			_customApiClientWriter?.WriteCodeAfterApiClientClass(_result, _apiClientModule);
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

			foreach (var enumType in enumTypes)
			{
				var enumPath = $"{enumsModuleName}/{enumType.Name}";
				_result.AppendLine($"import {{ {enumType.Name} }} from './{enumPath}'");
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
		private readonly IApiClientWriter _customApiClientWriter;
		private readonly Func<object, string> _serializeToJson;
		private readonly string _transportModuleName;
		private readonly IIndentedStringBuilder _result = new IndentedStringBuilder();
		
		public const string TransportContractsModuleName = "transport-contracts";		
	}
}
