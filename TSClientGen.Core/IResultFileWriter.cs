using TSClientGen.Extensibility;

namespace TSClientGen
{
	/// <summary>
	/// Writes content to the resulting TypeScript files 
	/// </summary>
	public interface IResultFileWriter
	{
		/// <summary>
		/// Writes a built-in module from assembly embedded resource to disk 
		/// </summary>
		/// <param name="moduleName">name of the module</param>
		void WriteBuiltinModule(string moduleName);

		/// <summary>
		/// Writes a file with a specified contents to disk.
		/// If the file exists it will be overwritten. 
		/// </summary>
		/// <param name="filename">name of the file relative to the output folder</param>
		/// <param name="contents">file contents to write</param>
		void WriteFile(string filename, string contents);

		/// <summary>
		/// Determines whether a resource module writer factory has been provided to TSClientGen via a plugin
		/// </summary>
		bool CanWriteResourceFiles { get; }
		
		/// <summary>
		/// Creates a resource module writer to write to a resource file with specified name and culture.
		/// The exact filename is determined by the <see cref="IResourceModuleWriterFactory"/> implementation.
		/// If the file exists it will be overwritten. 
		/// </summary>
		/// <param name="filename">name of the resource file relative to the output folder (not the exact filename, will be altered with the culture name)</param>
		/// <param name="culture">resource file culture name</param>
		IResourceModuleWriter WriteResourceFile(string filename, string culture);

		/// <summary>
		/// Removes all files from the output folder that were not generated as a result of the current TSClientGen run
		/// </summary>
		void CleanupOutDir();
	}
}