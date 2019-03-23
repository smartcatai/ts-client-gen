namespace TSClientGen.Extensibility
{
	/// <summary>
	/// Factory for creating instances of classes responsible for generating resource files
	/// </summary>
	public interface IResourceModuleWriterFactory
	{
		/// <summary>
		/// Creates an instance of resource module writer
		/// </summary>
		/// <param name="outDir">folder to place generated resource file to</param>
		/// <param name="baseFilename">base name of the resource file to generate (without culture and extension)</param>
		/// <param name="culture">culture of the resource file</param>
		/// <param name="defaultCulture">default culture</param>
		IResourceModuleWriter Create(string outDir, string baseFilename, string culture, string defaultCulture);
	}
}