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
	public class ModuleDescriptor
	{
		public ModuleDescriptor(
			string apiClientClassName,
			IReadOnlyCollection<MethodDescriptor> methods,
			Type controllerType)
		{
			ApiClientClassName = apiClientClassName;
			Methods = methods;

			var tsModuleAttribute = controllerType.GetCustomAttribute<TSModuleAttribute>();
			if (tsModuleAttribute == null)
				throw new ArgumentException("TSModuleAttribute must be applied to the controller type");

			Name = tsModuleAttribute.ModuleName;
			AdditionalTypes = controllerType.GetCustomAttributes<TSRequireTypeAttribute>().Select(a => a.GeneratedType).ToList();
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
		public IReadOnlyCollection<MethodDescriptor> Methods { get; }
		
		/// <summary>
		/// List of types that were explicitly added for this module
		/// </summary>
		public IReadOnlyCollection<Type> AdditionalTypes { get; }
		
		/// <summary>
		/// Whether a constructor of this api client class will take a parameter
		/// containing the target hostname against which the requests will be issued
		/// </summary>
		public bool SupportsExternalHost { get; }
	}
}