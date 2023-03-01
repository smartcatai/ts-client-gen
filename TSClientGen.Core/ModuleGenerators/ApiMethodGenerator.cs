using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class ApiMethodGenerator
	{
		public ApiMethodGenerator(ApiMethod apiMethod, IIndentedStringBuilder result, TypeMapping typeMapping)
		{
			_apiMethod = apiMethod;
			_result = result;
			_typeMapping = typeMapping;
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

		public void WriteBody(bool generateGetUrl, bool supportsExternalHost)
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

			_result.AppendLine(supportsExternalHost
				? $"const url = (this.hostname || '') + `{url}`;"
				: $"const url = `{url}`;");

			var requestParams = new List<string> {"url"};

			if (_apiMethod.QueryParams.Any())
			{
				requestParams.Add("queryStringParams");

				//Если параметр один и является классом - необходимо вернуть только сам параметр
				if (_apiMethod.QueryParams.Count == 1 && !_typeMapping.IsPrimitiveTsType(_apiMethod.QueryParams.First().Type))
				{
					_result.AppendLine($"const queryStringParams = {_apiMethod.QueryParams.First().GeneratedName};");
				}
				else
				{
					var queryParams = _apiMethod.QueryParams.Select(p =>
					{
						//Генерация параметров для классов - необходимо сгенировать строку для каждого поля
						if (!_typeMapping.IsPrimitiveTsType(p.Type))
						{
							return generateParametersForClass(p.Type, p.GeneratedName);
						}
					
						if (p.OriginalName == p.GeneratedName && p.Type != typeof(DateTime))
							return p.OriginalName;

						if (p.Type == typeof(DateTime))
							return $"{p.OriginalName}: {p.GeneratedName}.toISOString()";

						return $"{p.OriginalName}: {p.GeneratedName}";
					});
					_result.AppendLine($"const queryStringParams = {{ {string.Join(", ", queryParams)} }};");
				}
			}

			if (generateGetUrl)
			{
				_result.AppendLine($"return getUri({{ {string.Join(", ", requestParams)} }});");
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
				requestParams.Add("timeout");
			}

			requestParams.Add("method");
			_result.AppendLine($"const method = '{_apiMethod.HttpMethod.Method.ToLower()}';");

			requestParams.Add("jsonResponseExpected");
			string tsReturnType = _typeMapping.GetTSType(_apiMethod.ReturnType);
			bool jsonResponseExpected = (tsReturnType != "void");
			_result.AppendLine($"const jsonResponseExpected = {jsonResponseExpected.ToString().ToLower()};");
			_result.AppendLine($"return request<{tsReturnType}>({{ {string.Join(", ", requestParams)} }});");
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
				? "{ getAbortFunc, onUploadProgress, timeout }: UploadFileHttpRequestOptions = {}"
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
					new[] {"files", "getAbortFunc", "onUploadProgress", "timeout", "url", "method", "queryStringParams", "requestBody", "blob"}));

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

		private string generateParametersForClass(Type type, string parameterName)
		{
			var properties = getTypeProperties(type);
			var objectProperties = new StringBuilder();
			foreach (var property in properties)
			{
				var attr = property.GetCustomAttribute<DataMemberAttribute>();
				var propertyName = attr != null? attr.Name : char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
					
				objectProperties.Append($"{propertyName}: {parameterName}.{propertyName}, ");
			}

			return objectProperties.ToString().Remove(objectProperties.Length - 2);
		}
		
		private static PropertyInfo[] getTypeProperties(Type type)
		{
			var actualType = type;
			if (type.IsGenericType &&
			    (type.GetGenericTypeDefinition() == typeof(Nullable<>) ||
			     type.GetGenericTypeDefinition() == typeof(Task<>)))
			{
				actualType = type.GetGenericArguments()[0];
			}

			return actualType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		}
		
		private readonly ApiMethod _apiMethod;		
		private readonly IIndentedStringBuilder _result;
		private readonly TypeMapping _typeMapping;
	}
}