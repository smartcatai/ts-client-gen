using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TSClientGen.Extensibility.ApiDescriptors
{
	/// <summary>
	/// Describes a generated typescript module with an exported api client class.
	/// Typically corresponds to a single api controller on the backend.
	/// </summary>
	public class ApiClientModule
	{
		public ApiClientModule(
			string moduleName,
			string apiClientClassName,
			IReadOnlyCollection<ApiMethod> methods,
			Type controllerType)
		{
			Name = moduleName;
			ApiClientClassName = apiClientClassName;
			Methods = methods;

			var additionalTypes = new List<Type>();
			foreach (var requireTypeAttr in controllerType.GetCustomAttributes<TSRequireTypeAttribute>())
			{
				additionalTypes.Add(requireTypeAttr.GeneratedType);
				if (requireTypeAttr.IncludeDescendantsFromAssembly != null)
				{
					var targetAsm = requireTypeAttr.IncludeDescendantsFromAssembly.Assembly;
					var descendants = targetAsm.GetTypes().Where(requireTypeAttr.GeneratedType.IsAssignableFrom);
					additionalTypes.AddRange(descendants);
				}
			}
			ExplicitlyRequiredTypes = additionalTypes;
			
			SupportsExternalHost = controllerType.GetCustomAttribute<TSSupportsExternalHostAttribute>() != null;			
			
		}
		
		
		/// <summary>
		/// Module name
		/// </summary>
		public string Name { get; }
		
		/// <summary>
		/// Name of the exported api client class
		/// </summary>
		public string ApiClientClassName { get; }
				
		/// <summary>
		/// Describe methods of an api client. Each method corresponds to a single api method on the backend.
		/// </summary>
		public IReadOnlyCollection<ApiMethod> Methods { get; }
		
		/// <summary>
		/// List of types that were explicitly added for this module
		/// </summary>
		public IReadOnlyCollection<Type> ExplicitlyRequiredTypes { get; }
		
		/// <summary>
		/// Whether a constructor of this api client class will take a parameter
		/// containing the target hostname against which the requests will be issued
		/// </summary>
		public bool SupportsExternalHost { get; }
	}
}