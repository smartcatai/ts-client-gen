using System;
using System.Resources;

namespace TSClientGen
{
	/// <summary>
	/// For applying to assembly.
	/// Generates a resources file for consuming on frontend side from the specified server-side resx file.
	/// You should provide a plugin that handles resource file generation for the frontend when using this attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class TSExposeResxAttribute : Attribute
	{
		public TSExposeResxAttribute(Type resxType)
		{
			ResxName = resxType.Name;
			ResourceManager = resxType.GetResourceManager();
		}

		/// <summary>
		/// <see cref="ResourceManager"/> instance for the server-side resx file to be exposed to frontend
		/// </summary>
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Name of the server-side resx file
		/// </summary>
		public string ResxName { get; }
	}
}