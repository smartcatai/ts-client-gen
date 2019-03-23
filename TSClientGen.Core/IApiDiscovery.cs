using System;
using System.Collections.Generic;
using System.Reflection;
using TSClientGen.ApiDescriptors;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	/// <summary>
	/// Discovers api from an assembly 
	/// </summary>
	public interface IApiDiscovery
	{
		/// <summary>
		/// Constructs module descriptors for the api found in the assembly 
		/// </summary>
		IEnumerable<ModuleDescriptor> GetModules(Assembly assembly, Func<Type, bool> processModule);
	}
}