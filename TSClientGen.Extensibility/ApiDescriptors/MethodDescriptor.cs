using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TSClientGen.Extensibility.ApiDescriptors
{
	/// <summary>
	/// Describes an api method in a module
	/// </summary>
	public class MethodDescriptor
	{
		public MethodDescriptor(
			string name,
			string urlTemplate,
			string httpVerb,
			IReadOnlyCollection<MethodParamDescriptor> parameters,
			Type returnType,
			bool uploadsFiles,
			bool generateUrl)
		{
			Name = name;
			UrlTemplate = urlTemplate;
			HttpVerb = httpVerb;
			ReturnType = returnType;
			UploadsFiles = uploadsFiles;
			GenerateUrl = generateUrl;
			
			AllParams = parameters;
			BodyParam = AllParams.SingleOrDefault(p => p.IsBodyContent);
			UrlParamsByPlaceholder = _routeParamPattern
				.Matches(UrlTemplate)
				.Cast<Match>()
				.ToDictionary(m => m.Value, m =>
				{
					var param = parameters.SingleOrDefault(p => p.OriginalName == m.Groups[1].Value);
					if (param == null)
					{
						throw new Exception(
							$"Unexpected route template: {UrlTemplate}. Param {m.Value} doesn't match any of method params.");
					}

					return param;
				});
			QueryParams = AllParams.Where(p => !p.IsBodyContent && UrlParamsByPlaceholder.Values.All(p2 => p2.OriginalName != p.OriginalName)).ToList();			
		}
		
		public MethodDescriptor(
			MethodInfo method,
			string urlTemplate,
			string httpVerb,
			IReadOnlyCollection<MethodParamDescriptor> parameters)
			: this(
				method.Name,
				urlTemplate,
				httpVerb,
				parameters,
				method.ReturnType,
				method.GetCustomAttribute<TSUploadFilesAttribute>() != null,
				method.GetCustomAttribute<TSGenerateUrlAttribute>() != null)
		{
		}

		
		/// <summary>
		/// Method name
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Method url template (can contain placeholders for method params in a form of {name:type}) 
		/// </summary>
		public string UrlTemplate { get; }
		
		/// <summary>
		/// Return type of the method
		/// </summary>
		public Type ReturnType { get; }
		
		/// <summary>
		/// HTTP request verb
		/// </summary>
		public string HttpVerb { get; }

		/// <summary>
		/// Whether to generate a method in an api client class that takes a set of parameters and generates full api method url
		/// </summary>
		public bool GenerateUrl { get; }
		
		/// <summary>
		/// All api method parameters
		/// </summary>
		public IReadOnlyCollection<MethodParamDescriptor> AllParams { get; }
		
		/// <summary>
		/// Method parameters that are to be passed by a query string
		/// </summary>
		public IReadOnlyCollection<MethodParamDescriptor> QueryParams { get; }

		/// <summary>
		/// Method parameters that are to be inserted into a url template's placeholders
		/// </summary>
		public IReadOnlyDictionary<string, MethodParamDescriptor> UrlParamsByPlaceholder { get; }

		/// <summary>
		/// Method parameter that is to be passed in a request body
		/// </summary>
		public MethodParamDescriptor BodyParam { get; }

		/// <summary>
		/// Whether this method uploads any files in a multipart data request to the server
		/// </summary>
		public bool UploadsFiles { get;  }

		
		private static readonly Regex _routeParamPattern = new Regex(@"\{(.*?)(:.*?)*\}", RegexOptions.Compiled);
	}
}