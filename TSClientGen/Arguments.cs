using CommandLine;

namespace TSClientGen
{
	class Arguments
	{
		[OptionArray('a', "asm")]
		public string[] AssembliesPath { get; set; }

		[Option('o', "out-dir")]
		public string OutputPath { get; set; }

		[OptionArray('n', "asm-name")]
		public string[] AssemblyNames { get; set; }

		[OptionArray('l', "loc-lang")]
		public string[] LocalizationLanguages { get; set; }
		
		[Option("default-locale")]
		public string DefaultLocale { get; set; }
	}
}