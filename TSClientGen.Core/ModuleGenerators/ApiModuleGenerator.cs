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
			string transportModuleName)
		{
			_apiClientModule = apiClientModule;
			_typeMapping = typeMapping;
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
				string separator = ", ";
				if (enumTypes.Count > 3)
					separator += Environment.NewLine + "\t";

				if (enumTypes.Count < 4)
				{
					_result.Append("import { ")
						.Append(string.Join(separator, enumTypes.Select(t => t.Name)))
						.AppendLine($" }} from './{enumsModuleName}'")
						.AppendLine();					
				}
				else
				{
					_result.AppendLine("import {");
					foreach (var enumType in enumTypes)
					{
						_result.Append("\t").Append(enumType.Name).AppendLine(",");
					}
					_result.AppendLine($"}} from './{enumsModuleName}'");
				}
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
		private readonly string _transportModuleName;
		private readonly IndentedStringBuilder _result = new IndentedStringBuilder();
		
		public const string TransportContractsModuleName = "transport-contracts";		
	}
}