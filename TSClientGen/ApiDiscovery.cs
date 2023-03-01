using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	/// <summary>
	/// Searches assembly for the asp.net webapi controllers and generates module descriptions 
	/// </summary>
	public class ApiDiscovery : IApiDiscovery
	{
		public ApiDiscovery(IMethodDescriptorProvider customMethodDescriptorProvider)
		{
			_customMethodDescriptorProvider = customMethodDescriptorProvider;
		}


		public IEnumerable<ApiClientModule> GetModules(Assembly assembly)
		{
			var controllerTypes = assembly.GetTypes()
				.Where(t => typeof(ControllerBase).IsAssignableFrom(t))
				.ToList();
			var anyModuleAttributes = controllerTypes.Any(t => t.GetCustomAttributes<TSModuleAttribute>().Any());

			foreach (var controllerType in controllerTypes)
			{
				var tsModuleAttribute = controllerType.GetCustomAttribute<TSModuleAttribute>();
				if (anyModuleAttributes && tsModuleAttribute == null)
					continue;

				var controllerName = controllerType.Name.Replace("Controller", string.Empty);

				var controllerRoute = controllerType.GetCustomAttributes<RouteAttribute>()
					.Select(route => route.Template)
					.FirstOrDefault();

				var moduleName = tsModuleAttribute?.ModuleName ?? controllerName;
				var apiClientClassName = controllerType.Name.Replace("Controller", "Client");
				var actions = controllerType
					.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToArray();
				var methods = actions
					.Select(a => tryDescribeApiMethod(a, controllerRoute, controllerName))
					.Where(a => a != null)
					.ToList();

				yield return new ApiClientModule(moduleName, apiClientClassName, methods, controllerType);
			}
		}


		private ApiMethod tryDescribeApiMethod(MethodInfo method, string routePrefix, string controllerName)
		{
			if (method.GetCustomAttribute<TSIgnoreAttribute>() != null)
				return null;

			var routeAttribute = method.GetCustomAttributes<RouteAttribute>().FirstOrDefault();
			var httpMethodAttribute = method.GetCustomAttributes<HttpMethodAttribute>().FirstOrDefault();
			var httpMethod = httpMethodAttribute?.HttpMethods.First() ?? "GET";

			var parameters = method.GetParameters()
				.Where(p => p.ParameterType != typeof(CancellationToken))
				.Where(p => p.GetCustomAttribute<TSIgnoreAttribute>() == null)
				.Select(p => new ApiMethodParam(
					p.Name,
					p.ParameterType,
					p.IsOptional,
					p.GetCustomAttributes<FromBodyAttribute>().Any()))
				.ToList();

			var controllerRoute = routePrefix.EndsWith('/') ? routePrefix : routePrefix + '/';
			var methodRoute = (httpMethodAttribute?.Template ?? routeAttribute?.Template) ?? string.Empty;
			methodRoute = methodRoute.StartsWith('/') ? methodRoute[1..] : methodRoute;
			
			string urlTemplate = $"{controllerRoute}{methodRoute}"
				.Replace("[controller]", controllerName)
				.Replace("[action]", method.Name);
			var descriptor = new ApiMethod(method, urlTemplate, new HttpMethod(httpMethod), parameters);
			if (_customMethodDescriptorProvider != null)
			{
				descriptor = _customMethodDescriptorProvider.DescribeMethod(method.DeclaringType, method, descriptor);
			}

			return descriptor;
		}

		private readonly IMethodDescriptorProvider _customMethodDescriptorProvider;
	}
}