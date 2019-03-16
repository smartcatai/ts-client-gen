using System.Resources;

namespace TSClientGen.AspNetWebApi
{
	public sealed class ResourceModuleWriter : IResourceModuleWriter
	{
		public ResourceModuleWriter(string filename)
		{
			_resxWriter = new ResXResourceWriter(filename);
			Filename = filename;
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