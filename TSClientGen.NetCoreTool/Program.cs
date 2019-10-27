using System;
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

			var serializeToJson = plugin.JsonSerializer != null
				? (Func<object, string>) plugin.JsonSerializer.Serialize
				: obj => JsonSerializer.Serialize(obj, obj.GetType());

			var runner = new Runner(
				arguments,
				apiDiscovery,
				new TypeConverter(plugin.TypeConverter),
				plugin.TypeDescriptorProvider,
				plugin.CustomApiClientWriter,
				resultFileWriter,
				serializeToJson);

			runner.Execute();
			return 0;
		}
	}
}