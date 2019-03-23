using System.Globalization;
using TSClientGen.Extensibility;

namespace TSClientGen.AspNetWebApi
{
	public class ResourceModuleWriterFactory : IResourceModuleWriterFactory
	{
		public IResourceModuleWriter Create(string outDir, string baseFilename, string culture, string defaultCulture)
		{
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
			string targetFileName = (culture == defaultCulture) ? $"{baseFilename}.resx" : $"{baseFilename}.{culture}.resx";
			return new ResourceModuleWriter(outDir, targetFileName);
		}
	}
}