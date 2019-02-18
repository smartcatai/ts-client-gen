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
		private ActionDescriptor(
			string routePrefix,
			RouteAttribute route,
			IActionHttpMethodProvider httpVerb,
			MethodInfo controllerMethod,
			TypeMapper mapper)
		{
			Name = controllerMethod.Name;
			HttpVerb = getVerb(httpVerb);
			RouteTemplate = (routePrefix + route.Template).Replace("{action}", controllerMethod.Name);
			ReturnType = mapper.GetTSType(controllerMethod.ReturnType);
			GenerateUrl = controllerMethod.GetCustomAttribute<TSGenerateUrlAttribute>() != null;

			var allParams = controllerMethod.GetParameters()
				.Where(p => p.ParameterType != typeof(CancellationToken))
				.Select(p => new ActionParamDescriptor(p, mapper))
				.ToList();
			
			RouteParamsBySections = _routeParamPattern
				.Matches(RouteTemplate)
				.Cast<Match>()
				.ToDictionary(m => m.Value, m =>
				{
					var param = allParams.SingleOrDefault(p => p.Name == m.Groups[1].Value);
					if (param == null)
					{
						throw new Exception(
							$"Unexpected route template: {RouteTemplate}. Param {m.Value} doesn't match any of method params.");
					}

					return param;
				});

			BodyParam = allParams.SingleOrDefault(p => p.IsBodyContent);
			QueryParams = allParams.Where(p => !p.IsBodyContent && RouteParamsBySections.Values.All(p2 => p2.Name != p.Name)).ToList();
			AllParams = allParams;
		}

		public string Name { get; }
		
		public IReadOnlyCollection<ActionParamDescriptor> QueryParams { get; }

		public IReadOnlyDictionary<string, ActionParamDescriptor> RouteParamsBySections { get; }

		public ActionParamDescriptor BodyParam { get; }

		public bool IsModelWithFiles => BodyParam?.DotNetType.Name.StartsWith("ModelWithFiles") ?? false;

		public bool IsUploadedFile => BodyParam?.DotNetType.Name.StartsWith("UploadedFile") ?? false;

		public IReadOnlyCollection<ActionParamDescriptor> AllParams { get; }

		public string ReturnType { get; }
		
		public string RouteTemplate { get; }

		public string HttpVerb { get; }

		public bool GenerateUrl { get; }


		public static ActionDescriptor TryCreateFrom(
			MethodInfo controllerMethod,
			RouteAttribute controllerRoute,
			string routePrefix,
			TypeMapper mapper)
		{
			var route = controllerMethod.GetCustomAttributes<RouteAttribute>().SingleOrDefault() ?? controllerRoute;
			if (route == null)
				return null;

			var httpVerb = controllerMethod.GetCustomAttributes().OfType<IActionHttpMethodProvider>().FirstOrDefault();
			if (httpVerb == null)
				return null;

			return new ActionDescriptor(
				routePrefix,
				route,
				httpVerb,
				controllerMethod,
				mapper);
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