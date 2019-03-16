namespace TSClientGen
{
	public interface IArguments
	{
		string OutputPath { get; set; }
		string[] AssemblyPaths { get; set; }
		string[] AssemblyNames { get; set; }
		string CommonModuleName { get; set; }
		string[] LocalizationLanguages { get; set; }
		string DefaultLocale { get; set; }
	}
}