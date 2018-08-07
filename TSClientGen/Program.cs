using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using CommandLine;

namespace TSClientGen
{
	class Program
	{
		static int Main(string[] args)
		{
			var arguments = new Arguments();
			if (!Parser.Default.ParseArguments(args, arguments))
			{
				return 1;
			}

			if (Directory.Exists(arguments.ControllerClientsOutputDirPath))
				Directory.Delete(arguments.ControllerClientsOutputDirPath, true);

			Directory.CreateDirectory(arguments.ControllerClientsOutputDirPath);

			if (Directory.Exists(arguments.EnumsOutputDirPath))
				Directory.Delete(arguments.EnumsOutputDirPath, true);

			Directory.CreateDirectory(arguments.EnumsOutputDirPath);

			if (!Directory.Exists(arguments.ResourcesOutputDirPath))
				Directory.CreateDirectory(arguments.ResourcesOutputDirPath);

			var enumMapper = new EnumMapper(arguments);
			var enumStaticMemberProviders = new List<TSExtendEnumAttribute>();

			foreach (var asmPath in arguments.AssembliesPath)
			{
				var asm = Assembly.LoadFrom(asmPath);
				generateClientsFromAsm(asm, arguments.ControllerClientsOutputDirPath, arguments.ForVueApp, enumMapper);
				generateResources(asm.GetCustomAttributes().OfType<TSExposeResxAttribute>().ToList(), arguments);

				foreach (var attr in asm.GetCustomAttributes().OfType<TSExtendEnumAttribute>())
				{
					enumStaticMemberProviders.Add(attr);
					enumMapper.SaveEnum(asm, attr.EnumType);
				}

				foreach (var attr in asm.GetCustomAttributes().OfType<TSEnumModuleAttribute>())
				{
					enumMapper.SaveEnum(asm, attr.EnumType);
				}
			}

			appendEnumImports(enumMapper, arguments.ControllerClientsOutputDirPath);

			generateEnumsDefinition(enumMapper, enumStaticMemberProviders, arguments);

			return 0;
		}

		private static void appendEnumImports(EnumMapper enumMapper, string outDir)
		{
			var mapper = new TypeMapper();
			var imports = new StringBuilder(2048);

			foreach (var enumsByModule in enumMapper.GetReferencedEnumsByControllerModules())
			{
				var enumsByAsm = enumsByModule.GroupBy(e => e.EnumModuleName);

				foreach (var enums in enumsByAsm)
				{
					imports.Append("import { ");

					var isFirst = true;
					foreach (var @enum in enums)
					{
						if (isFirst)
							isFirst = false;
						else
							imports.Append(", ");

						imports.Append(mapper.GetTSType(@enum.EnumType));
					}

					imports.AppendLine($" }} from 'enums/{enums.Key}'");
				}

				string moduleFileName = Path.Combine(outDir, $"{enumsByModule.Key}.ts");
				if (!File.Exists(moduleFileName))
					moduleFileName = Path.Combine(outDir, $"{enumsByModule.Key}.d.ts");
				File.AppendAllText(moduleFileName, imports.ToString());
				imports.Clear();
			}
		}

		private static void generateEnumsDefinition(EnumMapper enumMapper, IReadOnlyCollection<TSExtendEnumAttribute> staticMemberProviders, Arguments args)
		{
			var sw = new Stopwatch();
			sw.Start();

			var typeMapper = new TypeMapper();
			var enumsDefinition = new StringBuilder(2048);
			var staticMemberProvidersLookup = staticMemberProviders.ToLookup(e => e.EnumType);

			foreach (var enums in enumMapper.GetEnumsByModules())
			{
				enumsDefinition.GenerateEnums(enums, staticMemberProvidersLookup, typeMapper, args.ForVueApp);
				File.WriteAllText(Path.Combine(args.EnumsOutputDirPath, $"{enums.Key}.ts"), enumsDefinition.ToString());
				enumsDefinition.Clear();
			}

			foreach (var culture in args.LocalizationLanguages)
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
				using (var enumResxFileWriter = new ResXResourceWriter(Path.Combine(args.ResourcesOutputDirPath, (culture == "ru") ? "enums.resx" : $"enums.{culture}.resx")))
				{
					enumResxFileWriter.GenerateEnumLocalizations(
						staticMemberProviders.OfType<TSEnumLocalizationAttribute>().ToList());
				}
			}

			Console.WriteLine($"Enums generated in {sw.ElapsedMilliseconds} ms");
		}

		private static void generateResources(IReadOnlyCollection<TSExposeResxAttribute> resources, Arguments args)
		{
			foreach (var culture in args.LocalizationLanguages)
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

				foreach (var resource in resources)
				{
					var resxName = culture == "ru" ? $"{resource.ResxName}.resx" : $"{resource.ResxName}.{culture}.resx";
					using (var resxFileWriter = new ResXResourceWriter(Path.Combine(args.ResourcesOutputDirPath, resxName)))
					{
						var resourceSet = resource.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
						foreach (DictionaryEntry entry in resourceSet)
						{
							resxFileWriter.AddResource(entry.Key.ToString(), entry.Value);
						}
					}					
				}
				
			}
		}

		private static void generateClientsFromAsm(Assembly targetAsm, string outDir, bool forVueApp, EnumMapper enumMapper)
		{
			var sw = new Stopwatch();
			sw.Start();

			var moduleNames = new HashSet<string>();

			var controllers = targetAsm.GetTypes()
				.Where(t => typeof(IHttpController).IsAssignableFrom(t) || typeof(IController).IsAssignableFrom(t));

			var staticContentByModuleName = targetAsm.GetCustomAttributes().OfType<TSStaticContentAttribute>()
				.ToDictionary(attr => attr.ModuleName);

			// Генерация клиентов к контроллерам
			foreach (var controller in controllers)
			{
				var tsModuleAttribute = controller.GetCustomAttributes<TSModuleAttribute>().SingleOrDefault();
				var moduleName = tsModuleAttribute?.ModuleName;
				if (moduleName != null)
				{
					if (moduleNames.Contains(moduleName))
						throw new Exception("Duplicate module name - " + moduleName);
					moduleNames.Add(moduleName);
				}

				var mapper = new TypeMapper();

				var additionalTypes = controller.GetCustomAttributes<RequireTSTypeAttribute>();
				foreach (var typeAttribute in additionalTypes)
				{
					if (typeAttribute.GeneratedType.IsEnum)
					{
						enumMapper.SaveEnum(targetAsm, typeAttribute.GeneratedType);
					}
					else
					{
						if (moduleName == null || !typeof(IHttpController).IsAssignableFrom(controller))
							throw new Exception($"Controller {controller.FullName} has invalid TypeScriptType attributes. Only enum types can be specified for MVC controllers or API controllers without TypeScriptModule attribute.");

						mapper.AddType(typeAttribute.GeneratedType);
					}
				}

				if (moduleName == null)
					continue;

				bool loadedAsJsonModule = tsModuleAttribute.LoadedAsJsonModule && !forVueApp;
				var result = new StringBuilder(2048);
				var controllerRoute = controller.GetCustomAttributes<System.Web.Http.RouteAttribute>().SingleOrDefault();
				if (loadedAsJsonModule)
				{
					result.GenerateJsonModule(controller, mapper, controllerRoute);
				}
				else
				{
					result.GenerateControllerClient(controller, mapper, controllerRoute);
				}

				var typesToGenerate = new HashSet<Type>();
				var generatedTypes = new HashSet<Type>();

				do
				{
					var knownTypes = mapper.GetCustomTypes();
					typesToGenerate.UnionWith(knownTypes);
					typesToGenerate.ExceptWith(generatedTypes);
					
					foreach (var typeDescriptor in typesToGenerate.Select(t => mapper.GetDescriptorByType(t)))
					{
						if (typeDescriptor.Type.IsEnum)
						{
							enumMapper.SaveEnum(targetAsm, moduleName, typeDescriptor.Type);
						}
						else
						{
							if (!string.IsNullOrWhiteSpace(typeDescriptor.TypeDefinition))
							{
								result.GenerateTypeDefinition(typeDescriptor, mapper, !loadedAsJsonModule);
							}else
							{
								result.GenerateInterface(typeDescriptor, mapper, !loadedAsJsonModule);
							}
						}
						generatedTypes.Add(typeDescriptor.Type);
					}
				} while (typesToGenerate.Any());

				if (!loadedAsJsonModule)
				{
					TSStaticContentAttribute staticContentModule;
					if (staticContentByModuleName.TryGetValue(moduleName, out staticContentModule))
					{
						result.GenerateStaticContent(staticContentModule);
					}
				}

				string extension = loadedAsJsonModule ? "d.ts" : "ts";
				File.WriteAllText(Path.Combine(outDir, $"{moduleName}.{extension}"), result.ToString());

				Console.WriteLine($"TypeScript client `{moduleName}` generated in {sw.ElapsedMilliseconds} ms");
				sw.Restart();
			}

			foreach (var staticContent in staticContentByModuleName.Values.Where(attr => !moduleNames.Contains(attr.ModuleName)))
			{
				if (moduleNames.Contains(staticContent.ModuleName))
					throw new Exception("Duplicate module name - " + staticContent.ModuleName);

				moduleNames.Add(staticContent.ModuleName);

				var result = new StringBuilder();
				result.GenerateStaticContent(staticContent);
				File.WriteAllText(Path.Combine(outDir, $"{staticContent.ModuleName}.ts"), result.ToString());
			}
		}
	}
}
