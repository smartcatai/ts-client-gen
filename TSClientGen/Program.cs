using System;
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
			var argsParseResult = Parser.Default.ParseArguments<Arguments>(args);
			if (argsParseResult is NotParsed<Arguments> notParsed)
			{
				foreach (var error in notParsed.Errors)
				{
					Console.WriteLine(error.ToString());
				}

				return 1;
			}

			var arguments = ((Parsed<Arguments>) argsParseResult).Value;
			if (arguments.BuiltinTransportModule == null && arguments.CustomTransportModule == null)
			{
				Console.WriteLine("Specify either --transport or --custom-transport command-line option");
				return 1;
			}

			var plugins = new InjectedPlugins();
			if (arguments.PluginsAssembly != null)
			{
				var pluginsAssembly = Assembly.LoadFrom(arguments.PluginsAssembly);
				using (var compositionContainer = new CompositionContainer(new AssemblyCatalog(pluginsAssembly)))
				{
					compositionContainer.ComposeParts(plugins);
				}
			}

			var apiDiscovery = new ApiDiscovery(plugins.MethodDescriptorProvider);
			var resultFileWriter = new ResultFileWriter(
				arguments.OutDir,
				arguments.DefaultLocale,
				plugins.ResourceModuleWriterFactory ?? new ResourceModuleWriterFactory());

			var runner = new Runner(
				arguments,
				apiDiscovery,
				new TypeConverter(plugins.TypeConverter),
				new TypeDescriptorProvider(plugins.TypeDescriptorProvider),
				resultFileWriter,
				JsonConvert.SerializeObject);

			runner.Execute();
			return 0;
		}
	}
}