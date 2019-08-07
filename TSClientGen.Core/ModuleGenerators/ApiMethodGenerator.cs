using System;
using System.Collections.Generic;
using System.Linq;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class ApiMethodGenerator
	{
		public ApiMethodGenerator(
			ApiMethod apiMethod,
			IndentedStringBuilder result,
			TypeMapping typeMapping,
			bool supportApiClientOptions)
		{
			_apiMethod = apiMethod;
			_result = result;
			_typeMapping = typeMapping;
			_supportApiClientOptions = supportApiClientOptions;
		}

		public void WriteSignature()
		{
			var parameters = GetTypescriptParams();
			_result
				.Append($"public {toLowerCamelCase(_apiMethod.Name)}(")
				.Append(string.Join(", ", parameters))
				.Append(")");
		}

		public void WriteGetUrlSignature()
		{
			var parameters = GetTypescriptParamsForUrl();
			_result
				.Append($"public {toLowerCamelCase(_apiMethod.Name)}Url(")
				.Append(string.Join(", ", parameters))
				.Append(")");
		}

		public void WriteBody(bool generateGetUrl)
		{
			string url = _apiMethod.UrlTemplate;
			foreach (var param in _apiMethod.UrlParamsByPlaceholder)
			{
				string paramValue = param.Value.GeneratedName;
				if (param.Value.Type == typeof(DateTime))
				{
					paramValue += ".toISOString()";
				}

				url = url.Replace(param.Key, "${" + paramValue + "}");
			}

			_result.AppendLine($"const url = (this.options ? this.options.hostname : '') + `{url}`;");

			var requestParams = new List<string> { "url" };
			string requestParamsObj;

			if (_apiMethod.QueryParams.Any())
			{
				requestParams.Add("queryStringParams");
				var queryParams = _apiMethod.QueryParams.Select(p =>
				{
					if (p.OriginalName == p.GeneratedName && p.Type != typeof(DateTime))
						return p.OriginalName;

					if (p.Type == typeof(DateTime))
						return $"{p.OriginalName}: {p.GeneratedName}.toISOString()";

					return $"{p.OriginalName}: {p.GeneratedName}";
				});
				_result.AppendLine($"const queryStringParams = {{ {string.Join(", ", queryParams)} }};");
			}

			if (generateGetUrl)
			{
				requestParamsObj = _supportApiClientOptions
					? $"{{ ...(this.options || {{}}), {string.Join(", ", requestParams)} }}"
					: $"{{ {string.Join(", ", requestParams)} }}";
				_result.AppendLine($"return getUri({requestParamsObj});");
				return;
			}

			if (_apiMethod.UploadsFiles)
			{
				requestParams.Add("requestBody");
				_result
					.AppendLine("const requestBody = new FormData();")
					.AppendLine("for (const f of files) {").Indent()
					.AppendLine("const namedBlob = f as NamedBlob;")
					.AppendLine("if (namedBlob.blob && namedBlob.name) {").Indent()
					.AppendLine("requestBody.append('file', namedBlob.blob, namedBlob.name);").Unindent()
					.AppendLine("} else {").Indent()
					.AppendLine("requestBody.append('file', f as File);").Unindent()
					.AppendLine("}").Unindent()
					.AppendLine("}");

				if (_apiMethod.BodyParam != null)
				{
					_result.AppendLine(
						$"const blob = new Blob([JSON.stringify({_apiMethod.BodyParam.GeneratedName})], {{ type: 'application/json' }});");
					_result.AppendLine("requestBody.append('Value', blob);");
				}
			}
			else if (_apiMethod.BodyParam != null)
			{
				requestParams.Add("requestBody");
				_result.AppendLine($"const requestBody = {_apiMethod.BodyParam.GeneratedName};");
			}

			requestParams.Add("getAbortFunc");
			if (_apiMethod.UploadsFiles)
			{
				requestParams.Add("onUploadProgress");
			}

			requestParams.Add("method");
			_result.AppendLine($"const method = '{_apiMethod.HttpMethod.Method.ToLower()}';");

			requestParams.Add("jsonResponseExpected");
			string tsReturnType = _typeMapping.GetTSType(_apiMethod.ReturnType);
			bool jsonResponseExpected = (tsReturnType != "void");
			_result.AppendLine($"const jsonResponseExpected = {jsonResponseExpected.ToString().ToLower()};");

			requestParamsObj = _supportApiClientOptions
				? $"{{ ...(this.options || {{}}), {string.Join(", ", requestParams)} }}"
				: $"{{ {string.Join(", ", requestParams)} }}";
			_result.AppendLine($"return request<{tsReturnType}>({requestParamsObj});");
		}

		public IEnumerable<string> GetTypescriptParams()
		{
			foreach (var param in _apiMethod.AllParams.OrderBy(p => p.IsOptional))
			{
				// files parameter is required and therefore has to go before all optional parameters
				if (param.IsOptional && _apiMethod.UploadsFiles)
					yield return "files: Array<NamedBlob | File>";

				yield return getTypescriptParam(param);
			}
			
			if (!_apiMethod.AllParams.Any(p => p.IsOptional) && _apiMethod.UploadsFiles)
				yield return "files: Array<NamedBlob | File>";

			yield return _apiMethod.UploadsFiles
				? "{ getAbortFunc, onUploadProgress }: UploadFileHttpRequestOptions = {}"
				: "{ getAbortFunc }: HttpRequestOptions = {}";
		}

		public IEnumerable<string> GetTypescriptParamsForUrl()
		{
			return from param in _apiMethod.AllParams
				where !_apiMethod.UploadsFiles || !param.IsBodyContent
				orderby param.IsOptional
				select getTypescriptParam(param);
		}

		public void ResolveConflictingParamNames(IEnumerable<string> moduleImports)
		{
			var identifiersInUse = new HashSet<string>(
				moduleImports.Concat(
					new[] {"files", "getAbortFunc", "onUploadProgress", "url", "method", "queryStringParams", "requestBody", "blob"}));

			foreach (var param in _apiMethod.AllParams.Where(param => !_apiMethod.UploadsFiles || !param.IsBodyContent))
			{
				while (identifiersInUse.Contains(param.GeneratedName))
					param.GeneratedName += "Param";

				identifiersInUse.Add(param.GeneratedName);
			}
		}

		
		private string getTypescriptParam(ApiMethodParam param)
		{
			var tsType = _typeMapping.GetTSType(param.Type);
			return $"{param.GeneratedName}{(param.IsOptional ? "?" : "")}: {tsType}";
		}
		
		private string toLowerCamelCase(string name)
		{
			return char.ToLowerInvariant(name[0]) + name.Substring(1);
		}
		
		
		private readonly ApiMethod _apiMethod;		
		private readonly IndentedStringBuilder _result;
		private readonly TypeMapping _typeMapping;
		private readonly bool _supportApiClientOptions;
	}
}