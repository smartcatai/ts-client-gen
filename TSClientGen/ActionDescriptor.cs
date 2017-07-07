using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace TSClientGen
{
	public class ActionDescriptor
	{
		private ActionDescriptor(RouteAttribute route, IActionHttpMethodProvider httpVerb, MethodInfo controllerMethod)
		{
			var allParams = controllerMethod.GetParameters().Where(p => p.ParameterType != typeof(CancellationToken)).ToArray();
			var queryParams = new List<ParameterInfo>();

			HttpVerb = getVerb(httpVerb);
			RouteTemplate = route.Template.Replace("{action}", controllerMethod.Name);
			GenerateUrl = controllerMethod.GetCustomAttribute<TypeScriptGenerateUrlAttribute>() != null;
			
			RouteParamsBySections = _routeParamPattern
				.Matches(RouteTemplate)
				.Cast<Match>()
				.ToDictionary(m => m.Value, m =>
				{
					var param = allParams.SingleOrDefault(p => p.Name == m.Groups[1].Value);
					if (param == null)
						throw new Exception(
							$"Unexpected route template: {RouteTemplate}. Param {m.Value} doesn't match any of method params.");

					return param;
				});

			foreach (var actionParam in allParams)
			{
				if (actionParam.GetCustomAttributes<FromBodyAttribute>().Any())
				{
					if (BodyParam != null)
						throw new Exception();

					BodyParam = actionParam;
				}
				else if (RouteParamsBySections.Values.All(p => p.Name != actionParam.Name))
				{
					queryParams.Add(actionParam);
				}
			}

			QueryParams = queryParams;

			AllParams = allParams;
		}

		public IReadOnlyCollection<ParameterInfo> QueryParams { get; }

		public IReadOnlyDictionary<string, ParameterInfo> RouteParamsBySections { get; }

		public ParameterInfo BodyParam { get; }

		public bool IsModelWithFiles => BodyParam?.ParameterType.Name.StartsWith("ModelWithFiles") ?? false;

		public bool IsUploadedFile => BodyParam?.ParameterType.Name.StartsWith("UploadedFile") ?? false;

		public IEnumerable<ParameterInfo> AllParams { get; }

		public string RouteTemplate { get; }

		public string HttpVerb { get; }

		public bool GenerateUrl { get; }
		
		public static ActionDescriptor TryCreateFrom(MethodInfo controllerMethod, RouteAttribute controllerRoute)
		{
			var route = controllerMethod.GetCustomAttributes<RouteAttribute>().SingleOrDefault() ?? controllerRoute;
			if (route == null)
				return null;

			var httpVerb = controllerMethod.GetCustomAttributes().OfType<IActionHttpMethodProvider>().FirstOrDefault();
			if (httpVerb == null)
				return null;

			return new ActionDescriptor(route, httpVerb, controllerMethod);
		}

		private static string getVerb(IActionHttpMethodProvider httpVerb)
		{
			if (httpVerb is HttpGetAttribute)
				return "GET";

			if (httpVerb is HttpPostAttribute)
				return "POST";

			if (httpVerb is HttpPutAttribute)
				return "PUT";

			if (httpVerb is HttpDeleteAttribute)
				return "DELETE";

			throw new Exception($"Unknown http verb: {httpVerb}");
		}

		private static readonly Regex _routeParamPattern = new Regex(@"\{(.*?)(:.*?)*\}", RegexOptions.Compiled);
	}
}