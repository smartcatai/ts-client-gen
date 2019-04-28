using CommandLine;

namespace TSClientGen
{
	class Arguments : IArguments
	{
		[OptionArray('a', "asm", Required = true)]
		public string[] AssemblyPaths { get; set; }

		[Option('o', "out-dir", Required = true)]
		public string OutDir { get; set; }
		
		[Option("cleanup-out-dir")]
		public bool CleanupOutDir { get; set; }
		
		[Option("append-i-prefix")]
		public bool AppendIPrefix { get; set; }

		[OptionArray("enum-module")]
		public string EnumsModuleName { get; set; }

		[Option("common-module")]
		public string CommonModuleName { get; set; }
		
		[Option("get-resource-module")]
		public string GetResourceModuleName { get; set; }		

		[OptionArray("loc-lang")]
		public string[] LocalizationLanguages { get; set; }
		
		[Option("default-locale")]
		public string DefaultLocale { get; set; }
		
		[Option('p', "plugins-assembly")]
		public string PluginsAssembly { get; set; }
	}
}