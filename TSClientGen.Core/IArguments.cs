namespace TSClientGen
{
	public interface IArguments
	{
		string OutDir { get; set; }
		bool CleanupOutDir { get; set; }
		string[] AssemblyPaths { get; set; }
		string EnumsModuleName { get; set; }
		string CommonModuleName { get; set; }
		string GetResourceModuleName { get; set; }
		string[] LocalizationLanguages { get; set; }
		string DefaultLocale { get; set; }
	}
}