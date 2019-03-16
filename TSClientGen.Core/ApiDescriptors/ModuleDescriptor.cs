using System.Collections.Generic;

namespace TSClientGen.ApiDescriptors
{
	/// <summary>
	/// Describes a generated typescript module with an exported api client class.
	/// Typically corresponds to a single api controller on the backend.
	/// </summary>
	public class ModuleDescriptor
	{
		public ModuleDescriptor(
			string name,
			string apiClientClassName,
			IReadOnlyCollection<MethodDescriptor> methods,
			TypeMapping typeMapping,
			bool supportsExternalHost)
		{
			Name = name;
			ApiClientClassName = apiClientClassName;
			Methods = methods;
			TypeMapping = typeMapping;
			SupportsExternalHost = supportsExternalHost;
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
		/// Stores a mapping from .net types used in the api module to corresponding TypeScript types
		/// </summary>
		public TypeMapping TypeMapping { get; }
		
		/// <summary>
		/// Whether a constructor of this api client class will take a parameter
		/// containing the target hostname against which the requests will be issued
		/// </summary>
		public bool SupportsExternalHost { get; }
	}
}