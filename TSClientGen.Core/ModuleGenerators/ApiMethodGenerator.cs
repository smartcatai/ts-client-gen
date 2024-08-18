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

		public void WriteBody(bool generateGetUrl)
		{
			if (generateGetUrl)
			{
				var requestParams = new List<string>
				{
					writeBaseURL(),
					writeUrl(),
					writeParams(),
				};
				_result.AppendLine($"return getUri({{ {string.Join(", ", requestParams.Where(p => p != null))} }});");
			}
			else
			{
				var requestParams = new List<string>
				{
					writeBaseURL(),
					writeUrl(),
					writeMethod(),
					writeHeaders(),
					writeData(),
					writeParams(),
					writeTimeout(),
					writeAbortSignal(),
					writeOnUploadProgress(),
				};
				var tsReturnType = _typeMapping.GetTSType(_apiMethod.ReturnType);
				_result.AppendLine($"return request<{tsReturnType}>({{ {string.Join(", ", requestParams.Where(p => p != null))} }});");
			}
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
				? "{ abortSignal, timeout, onUploadProgress }: UploadFileHttpRequestOptions = {}"
				: "{ abortSignal }: HttpRequestOptions = {}";
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
					new[] { "abortSignal", "blob", "data", "files", "method", "onUploadProgress", "params", "timeout", "url" }));

			foreach (var param in _apiMethod.AllParams.Where(param => !_apiMethod.UploadsFiles || !param.IsBodyContent))
			{
				while (identifiersInUse.Contains(param.GeneratedName))
					param.GeneratedName += "Param";

				identifiersInUse.Add(param.GeneratedName);
			}
		}

		private string writeBaseURL()
		{
			_result.AppendLine("const baseURL = this?.baseURL ?? '';");
			return "baseURL";
		}

		private string writeUrl()
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
			_result.AppendLine($"const url = `{url}`;");
			return "url";
		}

		private string writeMethod()
		{
			_result.AppendLine($"const method = '{_apiMethod.HttpMethod.Method.ToLower()}';");
			return "method";
		}

		private string writeHeaders()
		{
			_result.AppendLine("const headers = this?.headers ?? {};");
			return "headers";
		}

		private string writeData()
		{
			if (_apiMethod.UploadsFiles)
			{
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

				if (_apiMethod.BodyParam != null)
				{
					_result.AppendLine(
						$"const blob = new Blob([JSON.stringify({_apiMethod.BodyParam.GeneratedName})], {{ type: 'application/json' }});");
					_result.AppendLine("data.append('Value', blob);");
				}
				return "data";
			}
			else if (_apiMethod.BodyParam != null)
			{
				_result.AppendLine($"const data = {_apiMethod.BodyParam.GeneratedName};");
				return "data";
			}
			return null;
		}

		private string writeParams()
		{
			if (_apiMethod.QueryParams.Any())
			{
				var queryParams = _apiMethod.QueryParams.Select(p =>
				{
					//Генерация параметров для классов - необходимо сгенировать строку для каждого поля
					if (!_typeMapping.IsPrimitiveTsType(p.Type))
					{
						var generatedParameters = generateParametersForClass(p.Type, p.GeneratedName);
						if (!string.IsNullOrWhiteSpace(generatedParameters))
						{
							return generatedParameters;
						}
					}

					if (p.OriginalName == p.GeneratedName && p.Type != typeof(DateTime))
						return p.OriginalName;

					if (p.Type == typeof(DateTime))
						return $"{p.OriginalName}: {p.GeneratedName}.toISOString()";

					return $"{p.OriginalName}: {p.GeneratedName}";
				});
				_result.AppendLine($"const params = {{ {string.Join(", ", queryParams)} }};");
				return "params";
			}
			return null;
		}

		private string writeTimeout()
		{
			return _apiMethod.UploadsFiles ? "timeout" : null;
		}

		private string writeAbortSignal()
		{
			return "abortSignal";
		}

		private string writeOnUploadProgress()
		{
			return _apiMethod.UploadsFiles ? "onUploadProgress" : null;
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
			if (properties.Length <= 0) 
				return null;
			
			var objectProperties = new StringBuilder();
			var propertyCreated = false;
			foreach (var property in properties)
			{
				if (property.GetCustomAttributes<IgnoreDataMemberAttribute>().Any())
					continue;

				var propertyName = _typeMapping.GetPropertyName(property);
				objectProperties.Append($"{propertyName}: {parameterName}.{propertyName}");

				if (property.PropertyType == typeof(DateTime))
					objectProperties.Append(".toISOString()");

				objectProperties.Append(", ");
				propertyCreated = true;
			}
				
			return propertyCreated ? objectProperties.ToString().Remove(objectProperties.Length - 2) : null;
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