using Newtonsoft.Json;
using TSClientGen.NetCoreTool.AspNetCore;

namespace TSClientGen.NetCoreTool
{
	class Program
	{
		static int Main(string[] args)
		{
			var arguments = Runner.ParseArguments(args);
			if (arguments == null)
				return 1;

			var plugin = Runner.LoadPlugin(arguments);

			var apiDiscovery = plugin.ApiDiscovery ?? new ApiDiscovery(plugin.MethodDescriptorProvider);
			var resultFileWriter = new ResultFileWriter(arguments.OutDir, plugin.ResourceModuleWriterFactory);

			var runner = new Runner(
				arguments,
				apiDiscovery,
				new TypeConverter(plugin.TypeConverter),
				new TypeDescriptorProvider(plugin.TypeDescriptorProvider),
				resultFileWriter,
				JsonConvert.SerializeObject);

			runner.Execute();
			return 0;
		}
	}
}