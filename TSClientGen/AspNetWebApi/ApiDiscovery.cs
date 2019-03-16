using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using TSClientGen.ApiDescriptors;

namespace TSClientGen.AspNetWebApi
{
	/// <summary>
	/// Searches assembly for the asp.net webapi controllers and generates module descriptions 
	/// </summary>
	public class ApiDiscovery : IApiDiscovery
	{
		public IEnumerable<ModuleDescriptor> GetModules(
			Assembly assembly,
			EnumMapper enumMapper,
			ICustomTypeConverter customTypeConverter)
		{
			var controllerTypes = assembly.GetTypes().Where(t => typeof(IHttpController).IsAssignableFrom(t));
			foreach (var controllerType in controllerTypes)
			{
				var tsModuleAttribute = controllerType.GetCustomAttributes<TSModuleAttribute>().SingleOrDefault();
				if (tsModuleAttribute == null)
					continue;
			
				var controllerRoute = controllerType.GetCustomAttributes<RouteAttribute>().SingleOrDefault();
				var routePrefix = controllerType.GetCustomAttributes<RoutePrefixAttribute>().SingleOrDefault()?.Prefix;
				if (controllerRoute != null && routePrefix != null)
				{
					throw new Exception(
						$"Controller {controllerType.FullName} has Route and RoutePrefix attributes at the same time");
				}

				routePrefix = string.IsNullOrEmpty(routePrefix) ? "/" : "/" + routePrefix + "/";

				var additionalTypes = controllerType.GetCustomAttributes<RequireTSTypeAttribute>().ToList();
				foreach (var typeAttribute in additionalTypes.Where(a => a.GeneratedType.IsEnum))
				{
					enumMapper.SaveEnum(controllerType.Assembly, typeAttribute.GeneratedType);
				}
				
				var typeMapping = new TypeMapping(customTypeConverter);
				foreach (var typeAttribute in additionalTypes.Where(a => !a.GeneratedType.IsEnum))
				{
					typeMapping.AddType(typeAttribute.GeneratedType);
				}
			
				var moduleName = tsModuleAttribute.ModuleName;
				var apiClientClassName = controllerType.Name.Replace("Controller", "Client");
				var actions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToArray();
				var methods = actions
					.Select(a => tryDescribeApiMethod(a, controllerRoute, routePrefix, typeMapping))
					.Where(a => a != null)
					.ToList();
			
				var supportsExternalHost = controllerType.GetCustomAttributes<TSSupportsExternalHostAttribute>().Any();
			
				yield return new ModuleDescriptor(moduleName, apiClientClassName, methods, typeMapping, supportsExternalHost);				
			}
		}
		
		private MethodDescriptor tryDescribeApiMethod(
			MethodInfo method,
			RouteAttribute controllerRoute,
			string routePrefix,
			TypeMapping typeMapping)
		{
			var route = method.GetCustomAttributes<RouteAttribute>().SingleOrDefault() ?? controllerRoute;
			if (route == null)
				return null;

			var httpVerb = getVerb(method.GetCustomAttributes().OfType<IActionHttpMethodProvider>().FirstOrDefault());

			var parameters = method.GetParameters()
				.Where(p => p.ParameterType != typeof(CancellationToken))
				.Select(p => describeApiMethodParameter(p, typeMapping))
				.ToList();

			return new MethodDescriptor(
				method.Name,
				(routePrefix + route.Template).Replace("{action}", method.Name),
				httpVerb,
				parameters,
				typeMapping.GetTSType(method.ReturnType),
				method.GetCustomAttribute<TSGenerateUrlAttribute>() != null);
		}

		private MethodParamDescriptor describeApiMethodParameter(ParameterInfo parameter, TypeMapping typeMapping)
		{
			bool isBodyContent = parameter.GetCustomAttributes<FromBodyAttribute>().Any(); 
			return new MethodParamDescriptor(
				parameter.Name,
				typeMapping.GetTSType(parameter.ParameterType),
				parameter.IsOptional,
				isBodyContent,
				isBodyContent && parameter.Name.StartsWith("UploadedFile"),
				isBodyContent && parameter.Name.StartsWith("ModelWithFiles"));
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
	}
}