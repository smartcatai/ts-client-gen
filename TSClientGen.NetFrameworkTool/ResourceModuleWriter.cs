using System.IO;
using System.Resources;
using TSClientGen.Extensibility;

namespace TSClientGen.NetFrameworkTool
{
	public sealed class ResourceModuleWriter : IResourceModuleWriter
	{
		public ResourceModuleWriter(string outDir, string filename)
		{
			Filename = filename;
			_resxWriter = new ResXResourceWriter(Path.Combine(outDir, filename));
		}
		
		public string Filename { get; }
		
		public void AddResource(string key, string value)
		{
			_resxWriter.AddResource(key, value);
		}
		
		public void Dispose()
		{
			if (!_disposed)
			{
				_resxWriter?.Dispose();
				_disposed = true;
			}
		}

		private readonly ResXResourceWriter _resxWriter;
		private bool _disposed;
	}
}