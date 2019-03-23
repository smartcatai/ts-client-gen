using System;
using TSClientGen.Extensibility;

namespace TSClientGen
{
	public class ResourceFileWriter : IResourceModuleWriter
	{
		public ResourceFileWriter(IResourceModuleWriter wrapped, Action onDispose)
		{
			_wrapped = wrapped;
			_onDispose = onDispose;
		}

		public string Filename => _wrapped.Filename;

		public void AddResource(string key, string value)
		{
			_wrapped.AddResource(key, value);
		}
		
		public void Dispose()
		{
			_wrapped.Dispose();
			_onDispose();
		}

		
		private readonly IResourceModuleWriter _wrapped;
		private readonly Action _onDispose;
	}
}