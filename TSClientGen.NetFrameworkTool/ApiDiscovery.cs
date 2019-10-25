using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.NetFrameworkTool
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
				.Where(t => typeof(IHttpController).IsAssignableFrom(t))
				.ToList();
			var anyModuleAttributes = controllerTypes.Any(t => t.GetCustomAttributes<TSModuleAttribute>().Any());

			foreach (var controllerType in controllerTypes)
			{
				var tsModuleAttribute = controllerType.GetCustomAttribute<TSModuleAttribute>();
				if (anyModuleAttributes && tsModuleAttribute == null)
					continue;

				if (controllerType.GetCustomAttribute<TSIgnoreAttribute>() != null)
					continue;

				var controllerRoute = controllerType.GetCustomAttributes<RouteAttribute>().SingleOrDefault();
				var routePrefix = controllerType.GetCustomAttributes<RoutePrefixAttribute>().SingleOrDefault()?.Prefix;
				if (controllerRoute != null && routePrefix != null)
				{
					throw new Exception(
						$"Controller {controllerType.FullName} has Route and RoutePrefix attributes at the same time");
				}

				routePrefix = string.IsNullOrEmpty(routePrefix) ? "/" : "/" + routePrefix + "/";

				var moduleName = tsModuleAttribute?.ModuleName ??
				                 controllerType.Name.Replace("Controller", string.Empty);
				var apiClientClassName = controllerType.Name.Replace("Controller", "Client");
				var actions = controllerType
					.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToArray();
				var methods = actions
					.Select(a => tryDescribeApiMethod(a, controllerRoute, routePrefix))
					.Where(a => a != null)
					.ToList();

				yield return new ApiClientModule(moduleName, apiClientClassName, methods, controllerType);
			}
		}


		private ApiMethod tryDescribeApiMethod(MethodInfo method, RouteAttribute controllerRoute, string routePrefix)
		{
			var route = method.GetCustomAttributes<RouteAttribute>().SingleOrDefault() ?? controllerRoute;
			if (route == null)
				return null;

			if (method.GetCustomAttribute<TSIgnoreAttribute>() != null)
				return null;

			var httpMethod = method.GetCustomAttributes().OfType<IActionHttpMethodProvider>()
				.SelectMany(a => a.HttpMethods)
				.FirstOrDefault();
			if (httpMethod == null)
				throw new Exception(
					$"Can't determine http method for method {method.Name} in type {method.DeclaringType.FullName}");

			var parameters = method.GetParameters()
				.Where(p => p.ParameterType != typeof(CancellationToken))
				.Where(p => p.GetCustomAttribute<TSIgnoreAttribute>() == null)
				.Select(p => new ApiMethodParam(
					p.Name,
					p.ParameterType,
					p.IsOptional,
					p.GetCustomAttributes<FromBodyAttribute>().Any()))
				.ToList();

			string urlTemplate = (routePrefix + route.Template).Replace("{action}", method.Name);
			var descriptor = new ApiMethod(method, urlTemplate, httpMethod, parameters);
			if (_customMethodDescriptorProvider != null)
			{
				descriptor = _customMethodDescriptorProvider.DescribeMethod(method.DeclaringType, method, descriptor);
			}

			return descriptor;
		}

		private readonly IMethodDescriptorProvider _customMethodDescriptorProvider;
	}
}