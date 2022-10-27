using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TSClientGen.Extensibility;

namespace TSClientGen
{
	/// <summary>
	/// Writes content to the resulting TypeScript files 
	/// </summary>
	public class ResultFileWriter : IResultFileWriter
	{
		public ResultFileWriter(string outDir, string defaultLocale, IResourceModuleWriterFactory resourceModuleWriterFactory)
		{
			_outDir = outDir;
			_defaultLocale = defaultLocale;
			_resourceModuleWriterFactory = resourceModuleWriterFactory;

			if (!Directory.Exists(_outDir))
				Directory.CreateDirectory(_outDir);
		}

		
		/// <summary>
		/// Writes a built-in module from assembly embedded resource to disk 
		/// </summary>
		/// <param name="moduleName">name of the module</param>
		public void WriteBuiltinModule(string moduleName)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"TSClientGen." + moduleName))
			using (var streamReader = new StreamReader(stream))
			{
				File.WriteAllText(
					Path.Combine(_outDir, moduleName),
					streamReader.ReadToEnd());
			}

			_generatedFiles.Add(Path.Combine(_outDir, moduleName).ToLowerInvariant());
		}

		/// <summary>
		/// Writes a file with a specified contents to disk.
		/// If the file exists it will be overwritten. 
		/// </summary>
		/// <param name="filename">name of the file relative to the output folder</param>
		/// <param name="contents">file contents to write</param>
		public void WriteFile(string filename, string contents)
		{
			var fullFilePath = Path.Combine(_outDir, filename);
			ensureDirectories(fullFilePath);

			File.WriteAllText(fullFilePath, contents);
			fixFilenameCase(filename);
			_generatedFiles.Add(fullFilePath.ToLowerInvariant());
		}

		/// <summary>
		/// Determines whether a resource module writer factory has been provided to TSClientGen via a plugin
		/// </summary>
		public bool CanWriteResourceFiles => _resourceModuleWriterFactory != null;

		/// <summary>
		/// Creates a resource module writer to write to a resource file with specified name and culture.
		/// The exact filename is determined by the <see cref="IResourceModuleWriterFactory"/> implementation.
		/// If the file exists it will be overwritten. 
		/// </summary>
		/// <param name="filename">name of the resource file relative to the output folder (not the exact filename, will be altered with the culture name)</param>
		/// <param name="culture">resource file culture name</param>
		public IResourceModuleWriter WriteResourceFile(string filename, string culture)
		{
			var fullFilePath = Path.Combine(_outDir, filename);
			ensureDirectories(fullFilePath);

			var moduleWriter = _resourceModuleWriterFactory.Create(
				_outDir, filename, culture, _defaultLocale);
			return new ResourceFileWriter(moduleWriter, () =>
			{
				var fullFilename = Path.Combine(_outDir, moduleWriter.Filename);
				fixFilenameCase(moduleWriter.Filename);
				_generatedFiles.Add(fullFilename.ToLowerInvariant());
			});
		}

		/// <summary>
		/// Removes all files from the output folder that were not generated as a result of the current TSClientGen run
		/// </summary>
		public void CleanupOutDir()
		{
			var filesToDelete = Directory
				.EnumerateFiles(_outDir, "*.*", SearchOption.AllDirectories)
				.Where(file => !_generatedFiles.Contains(file.ToLowerInvariant()))
				.ToList();

			if (!filesToDelete.Any())
				return;

			Console.WriteLine("Cleaning up...");
			foreach (var existingFile in filesToDelete)
			{
				File.Delete(existingFile);
				Console.WriteLine($"\t{existingFile} deleted");
			}
		}
		
		
		private void fixFilenameCase(string targetFileName)
		{
			var existingFileName = Directory.EnumerateFiles(_outDir, targetFileName).Single();
			if (targetFileName != Path.GetFileName(existingFileName))
			{
				// filenames differ in case, renaming
				// casing of the output file names is important because webpack is case-sensitive
				new FileInfo(existingFileName).MoveTo(Path.Combine(_outDir, targetFileName));
			}
		}
		
		private void ensureDirectories(string path)
		{
			var folder = new FileInfo(path).Directory!.FullName;
			Directory.CreateDirectory(folder);
		}

		private readonly string _outDir;
		private readonly string _defaultLocale;
		private readonly IResourceModuleWriterFactory _resourceModuleWriterFactory;
		private readonly HashSet<string> _generatedFiles = new HashSet<string>();
	}
}