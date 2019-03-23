using System;

namespace TSClientGen.Extensibility
{
	/// <summary>
	/// Responsible for generating localization resource files
	/// </summary>
	public interface IResourceModuleWriter : IDisposable
	{
		/// <summary>
		/// Name of the generated file
		/// </summary>
		string Filename { get; }
		
		/// <summary>
		/// Appends a resource string to a generated file
		/// </summary>
		/// <param name="key">resource key</param>
		/// <param name="value">localized resource string</param>
		void AddResource(string key, string value);
	}
}