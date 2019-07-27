using Newtonsoft.Json;

namespace TSClientGen.NetFrameworkTool
{
	class Program
	{
		static int Main(string[] args)
		{
			var arguments = Runner.ParseArguments(args);
			var plugin = Runner.LoadPlugin(arguments);

			var apiDiscovery = plugin.ApiDiscovery ?? new ApiDiscovery(plugin.MethodDescriptorProvider);
			var resultFileWriter = new ResultFileWriter(
				arguments.OutDir,
				arguments.DefaultLocale,
				plugin.ResourceModuleWriterFactory ?? new ResourceModuleWriterFactory());

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