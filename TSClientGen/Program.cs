using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using CommandLine;
using Newtonsoft.Json;
using TSClientGen.AspNetWebApi;

namespace TSClientGen
{
	class Program
	{
		static int Main(string[] args)
		{
			var arguments = new Arguments();
			if (!Parser.Default.ParseArguments(args, arguments))
				return 1;

			var plugins = new InjectedPlugins();
			if (arguments.PluginsAssembly != null)
			{
				var pluginsAssembly = Assembly.LoadFrom(arguments.PluginsAssembly);
				using (var compositionContainer = new CompositionContainer(new AssemblyCatalog(pluginsAssembly)))
				{
					compositionContainer.ComposeParts(plugins);
				}
			}
			
			var runner = new Runner(
				arguments,
				new ApiDiscovery(plugins.MethodDescriptorProvider),
				new TypeConverter(plugins.TypeConverter),
				new TypeDescriptorProvider(plugins.TypeDescriptorProvider),
				plugins.ResourceModuleWriterFactory ?? new ResourceModuleWriterFactory(),
				JsonConvert.SerializeObject);
			
			runner.Execute();
			return 0;
		}
	}
}