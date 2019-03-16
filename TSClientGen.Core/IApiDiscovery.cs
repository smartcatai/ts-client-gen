using System.Collections.Generic;
using System.Reflection;
using TSClientGen.ApiDescriptors;

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
		IEnumerable<ModuleDescriptor> GetModules(
			Assembly assembly,
			EnumMapper enumMapper,
			ICustomTypeConverter customTypeConverter);
	}
}