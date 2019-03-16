using CommandLine;

namespace TSClientGen
{
	class Arguments : IArguments
	{
		[Option('o', "out-dir")]
		public string OutputPath { get; set; }

		[OptionArray('a', "asm")]
		public string[] AssemblyPaths { get; set; }

		[OptionArray('n', "asm-name")]
		public string[] AssemblyNames { get; set; }
		
		[Option('c', "common-module")]
		public string CommonModuleName { get; set; }

		[OptionArray('l', "loc-lang")]
		public string[] LocalizationLanguages { get; set; }
		
		[Option("default-locale")]
		public string DefaultLocale { get; set; }
	}
}