using System.Collections.Generic;

namespace TSClientGen
{
	public interface IArguments
	{
		string OutDir { get; set; }
		bool CleanupOutDir { get; set; }
		IEnumerable<string> AssemblyPaths { get; set; }
		string EnumsModuleName { get; set; }
		bool UseStringEnums { get; set; }
		BuiltinXhrModule? BuiltinTransportModule { get; set; }
		string CustomTransportModule { get; set; }
		bool AppendIPrefix { get; set; }
		string GetResourceModuleName { get; set; }
		IEnumerable<string> LocalizationLanguages { get; set; }
		string DefaultLocale { get; set; }
	}
}