using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.AspNetWebApi
{
	/// <summary>
	/// Searches assembly for the asp.net webapi controllers and generates module descriptions 
	/// </summary>
	public class ApiDiscovery : IApiDiscovery
	{
		public ApiDiscovery(ICustomMethodDescriptorProvider customMethodDescriptorProvider)
		{
			_customMethodDescriptorProvider = customMethodDescriptorProvider;
		}
		
		
		public IEnumerable<ModuleDescriptor> GetModules(Assembly assembly, Func<Type, bool> processModule)
		{
			var controllerTypes = assembly.GetTypes().Where(t => typeof(IHttpController).IsAssignableFrom(t));
			foreach (var controllerType in controllerTypes)
			{
				if (!processModule(controllerType))
					continue;
			
				var controllerRoute = controllerType.GetCustomAttributes<RouteAttribute>().SingleOrDefault();
				var routePrefix = controllerType.GetCustomAttributes<RoutePrefixAttribute>().SingleOrDefault()?.Prefix;
				if (controllerRoute != null && routePrefix != null)
				{
					throw new Exception(
						$"Controller {controllerType.FullName} has Route and RoutePrefix attributes at the same time");
				}

				routePrefix = string.IsNullOrEmpty(routePrefix) ? "/" : "/" + routePrefix + "/";
				
				var apiClientClassName = controllerType.Name.Replace("Controller", "Client");
				var actions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToArray();
				var methods = actions
					.Select(a => tryDescribeApiMethod(a, controllerRoute, routePrefix))
					.Where(a => a != null)
					.ToList();
			
				yield return new ModuleDescriptor(apiClientClassName, methods, controllerType);				
			}
		}
		
		
		private MethodDescriptor tryDescribeApiMethod(MethodInfo method, RouteAttribute controllerRoute, string routePrefix)
		{
			var route = method.GetCustomAttributes<RouteAttribute>().SingleOrDefault() ?? controllerRoute;
			if (route == null)
				return null;

			var httpVerb = getVerb(method.GetCustomAttributes().OfType<IActionHttpMethodProvider>().FirstOrDefault());
			
			var parameters = method.GetParameters()
				.Where(p => p.ParameterType != typeof(CancellationToken))
				.Select(p => new MethodParamDescriptor(
					p.Name,
					p.ParameterType,
					p.IsOptional,
					p.GetCustomAttributes<FromBodyAttribute>().Any()))
				.ToList();

			string urlTemplate = (routePrefix + route.Template).Replace("{action}", method.Name); 
			var descriptor = new MethodDescriptor(method, urlTemplate, httpVerb, parameters);
			if (_customMethodDescriptorProvider != null)
			{
				descriptor = _customMethodDescriptorProvider.DescribeMethod(method.DeclaringType, method, descriptor);
			}

			return descriptor;
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
		
		private readonly ICustomMethodDescriptorProvider _customMethodDescriptorProvider;		
	}
}