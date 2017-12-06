using System;

namespace TSClientGen
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TSModuleAttribute : Attribute
	{
		public TSModuleAttribute(string moduleName)
		{
			ModuleName = moduleName;
		}

		public string ModuleName { get; }

		public bool LoadedAsJsonModule { get; set; }
	}
}