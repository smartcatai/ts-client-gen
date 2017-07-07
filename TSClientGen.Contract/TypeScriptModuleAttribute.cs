using System;

namespace TSClientGen
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TypeScriptModuleAttribute : Attribute
	{
		public TypeScriptModuleAttribute(string moduleName)
		{
			ModuleName = moduleName;
		}

		public string ModuleName { get; }

		public bool LoadedAsJsonModule { get; set; }
	}
}