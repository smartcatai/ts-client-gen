using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to api controller.
	/// Specifies api client module name for this controller.
	/// Api client module won't be generated for a controller if it is not marked with this attribute.  
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TSModuleAttribute : Attribute
	{
		public TSModuleAttribute(string moduleName)
		{
			ModuleName = moduleName;
		}

		/// <summary>
		/// Generated api client module name
		/// </summary>
		public string ModuleName { get; }
	}
}