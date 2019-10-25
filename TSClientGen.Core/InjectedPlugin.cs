using System.ComponentModel.Composition;
using TSClientGen.Extensibility;

namespace TSClientGen
{
	public class InjectedPlugin
	{
		[Import(AllowDefault = true)]
		public IApiDiscovery ApiDiscovery { get; set; }

		[Import(AllowDefault = true)]
		public IMethodDescriptorProvider MethodDescriptorProvider { get; set; }

		[Import(AllowDefault = true)]
		public ITypeDescriptorProvider TypeDescriptorProvider { get; set; }

		[Import(AllowDefault = true)]
		public ITypeConverter TypeConverter { get; set; }

		[Import(AllowDefault = true)]
		public IResourceModuleWriterFactory ResourceModuleWriterFactory { get; set; }

		[Import(AllowDefault = true)]
		public IApiClientWriter CustomApiClientWriter { get; set; }
	}
}