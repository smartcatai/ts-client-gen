using System;
using System.Collections.Generic;
using System.Linq;
using TSClientGen.ApiDescriptors;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class ApiMethodGenerator
	{
		public ApiMethodGenerator(MethodDescriptor method, IndentedStringBuilder result, TypeMapping typeMapping)
		{
			_method = method;
			_result = result;
			_typeMapping = typeMapping;
		}

		public void WriteSignature()
		{
			var parameters = GetTypescriptParams();
			_result
				.Append($"public {toLowerCamelCase(_method.Name)}(")
				.Append(string.Join(", ", parameters))
				.Append(")");
		}

		public void WriteGetUrlSignature()
		{
			var parameters = GetTypescriptParamsForUrl();
			_result
				.Append($"public {toLowerCamelCase(_method.Name)}Url(")
				.Append(string.Join(", ", parameters))
				.Append(")");
		}

		public void WriteBody(bool generateGetUrl, bool supportsExternalHost)
		{
			_result.AppendLine($"const method = '{_method.HttpVerb.ToLower()}';");
		
			string url = _method.UrlTemplate;
			foreach (var param in _method.UrlParamsByPlaceholder)
			{
				string paramValue = param.Value.GeneratedName;
				if (param.Value.Type == typeof(DateTime))
				{
					paramValue += ".toISOString()";
				}
				url = url.Replace(param.Key,"${" + paramValue + "}");
			}
			
			_result.AppendLine(supportsExternalHost
				? $"const url = (this.hostname || '') + `{url}`;"
				: $"const url = `{url}`;");

			var requestParams = new List<string> { "url", "method" };
			
			if (_method.QueryParams.Any())
			{
				requestParams.Add("params");
				var queryParams = _method.QueryParams.Select(p =>
				{
					if (p.OriginalName == p.GeneratedName && p.Type != typeof(DateTime))
						return p.OriginalName;
					
					if (p.Type == typeof(DateTime))
						return $"{p.OriginalName}: {p.GeneratedName}.toISOString()";

					return $"{p.OriginalName}: {p.GeneratedName}";
				});
				_result.AppendLine($"const params = {{ {string.Join(", ", queryParams)} }};");
			}

			if (generateGetUrl)
			{
				_result.AppendLine($"return getUri({{ {string.Join(", ", requestParams)} }});");
				return;
			}
			
			if (_method.UploadsFiles)
			{
				requestParams.Add("data");
				_result
					.AppendLine("const data = new FormData();")
					.AppendLine("for (const f of files) {").Indent()
					.AppendLine("const namedBlob = f as NamedBlob;")
					.AppendLine("if (namedBlob.blob && namedBlob.name) {").Indent()
					.AppendLine("data.append('file', namedBlob.blob, namedBlob.name);").Unindent()
					.AppendLine("} else {").Indent()
					.AppendLine("data.append('file', f as File);").Unindent()
					.AppendLine("}").Unindent()
					.AppendLine("}");

				if (_method.BodyParam != null)
				{
					_result.AppendLine($"const blob = new Blob([JSON.stringify({_method.BodyParam.GeneratedName})], {{ type: 'application/json' }});");
					_result.AppendLine("data.append('Value', blob);");
				}
			}
			else if (_method.BodyParam != null)
			{
				requestParams.Add("data");
				_result.AppendLine($"const data = {_method.BodyParam.GeneratedName};");
			}

			requestParams.AddRange(new[] { "cancelToken" });
			if (_method.UploadsFiles)
			{
				requestParams.Add("onUploadProgress");
			}

			string tsReturnType = _typeMapping.GetTSType(_method.ReturnType);
			_result.AppendLine($"return request<{tsReturnType}>({{ {string.Join(", ", requestParams)} }});");
		}

		public IEnumerable<string> GetTypescriptParams()
		{
			foreach (var param in _method.AllParams.OrderBy(p => p.IsOptional))
			{
				// files parameter is required and therefore has to go before all optional parameters
				if (param.IsOptional && _method.UploadsFiles)
					yield return "files: Array<NamedBlob | File>";

				yield return getTypescriptParam(param);
			}
			
			if (!_method.AllParams.Any(p => p.IsOptional) && _method.UploadsFiles)
				yield return "files: Array<NamedBlob | File>";

			yield return _method.UploadsFiles
				? "{ cancelToken, onUploadProgress }: UploadFileHttpRequestOptions = {}"
				: "{ cancelToken }: HttpRequestOptions = {}";
		}

		public IEnumerable<string> GetTypescriptParamsForUrl()
		{
			return from param in _method.AllParams
				where !_method.UploadsFiles || !param.IsBodyContent
				orderby param.IsOptional
				select getTypescriptParam(param);
		}

		public void ResolveConflictingParamNames(IEnumerable<string> moduleImports)
		{
			var identifiersInUse = new HashSet<string>(
				moduleImports.Concat(
					new[] {"files", "cancelToken", "onUploadProgress", "url", "method", "params", "data", "blob"}));

			foreach (var param in _method.AllParams.Where(param => !_method.UploadsFiles || !param.IsBodyContent))
			{
				while (identifiersInUse.Contains(param.GeneratedName))
					param.GeneratedName += "Param";

				identifiersInUse.Add(param.GeneratedName);
			}
		}

		
		private string getTypescriptParam(MethodParamDescriptor param)
		{
			var tsType = _typeMapping.GetTSType(param.Type);
			return $"{param.GeneratedName}{(param.IsOptional ? "?" : "")}: {tsType}";
		}
		
		private string toLowerCamelCase(string name)
		{
			return char.ToLowerInvariant(name[0]) + name.Substring(1);
		}
		
		
		private readonly MethodDescriptor _method;		
		private readonly IndentedStringBuilder _result;
		private readonly TypeMapping _typeMapping;
	}
}