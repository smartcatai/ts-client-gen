using System;
using System.Reflection;

namespace TSClientGen
{
	public class FunctionsAssemblyResolver
	{
		static FunctionsAssemblyResolver()
		{
			AppDomain.CurrentDomain.AssemblyResolve += resolveCurrentDomainAssembly;
		}

		public static void Init() { }

		private static Assembly resolveCurrentDomainAssembly(object sender, ResolveEventArgs args)
		{
			var requestedAssembly = new AssemblyName(args.Name);
			try
			{
				return requestedAssembly.Name == "System.ComponentModel.Annotations"
					? Assembly.Load(requestedAssembly.Name)
					: null;
			}
			catch
			{
				// do nothing
			}

			return null;
		}
	}
}