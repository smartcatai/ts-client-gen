using System.Text.Json;

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

			var apiDiscovery = new ApiDiscovery(plugin.MethodDescriptorProvider);
			var resultFileWriter = new ResultFileWriter(
				arguments.OutDir,
				arguments.DefaultLocale,
				plugin.ResourceModuleWriterFactory);

			var runner = new Runner(
				arguments,
				apiDiscovery,
				new TypeConverter(plugin.TypeConverter),
				plugin.TypeDescriptorProvider,
				plugin.CustomApiClientWriter,
				resultFileWriter,
				obj => JsonSerializer.Serialize(obj, obj.GetType()));

			runner.Execute();
			return 0;
		}
	}
}