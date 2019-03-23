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
			
			var runner = new Runner(
				arguments,
				new ApiDiscovery(null),
				new TypeConverter(),
				new PropertyNameProvider(),
				new ResourceModuleWriterFactory(),
				JsonConvert.SerializeObject);
			runner.Execute();
			return 0;
		}
	}
}