using CommandLine;

namespace TSClientGen
{
	class Arguments
	{
		[OptionArray('a', "asm")]
		public string[] AssembliesPath { get; set; }

		[Option('c', "clients-out-dir")]
		public string ControllerClientsOutputDirPath { get; set; }

		[Option('e', "enums-out-dir")]
		public string EnumsOutputDirPath { get; set; }

		[Option('r', "res-out-dir")]
		public string ResourcesOutputDirPath { get; set; }

		[OptionArray('n', "asm-name")]
		public string[] AssemblyNames { get; set; }
	}
}