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
using System.Web.Http.Controllers;
using CommandLine;

namespace TSClientGen
{
	class Program
	{
		private const string CommonModuleName = "common.ts";

		static int Main(string[] args)
		{
			var arguments = new Arguments();
			if (!Parser.Default.ParseArguments(args, arguments))
			{
				return 1;
			}

			if (!Directory.Exists(arguments.OutputPath))
				Directory.CreateDirectory(arguments.OutputPath);				

			var enumMapper = new EnumMapper(arguments);
			var enumStaticMemberProviders = new List<TSExtendEnumAttribute>();

			var generatedFiles = new HashSet<string>();
			
			generateCommonModule(arguments.OutputPath, generatedFiles);

			foreach (var asmPath in arguments.AssembliesPath)
			{
				var asm = Assembly.LoadFrom(asmPath);
				generateClientsFromAsm(asm, arguments.OutputPath, enumMapper, generatedFiles);
				generateResources(
					asm,
					arguments.OutputPath,
					arguments.LocalizationLanguages,
					arguments.DefaultLocale,
					generatedFiles);

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

			appendEnumImports(enumMapper, arguments.OutputPath);

			generateEnumsDefinition(
				enumMapper,
				enumStaticMemberProviders,
				arguments.OutputPath,
				arguments.LocalizationLanguages,
				arguments.DefaultLocale,
				generatedFiles);

			cleanupOutDir(arguments.OutputPath, generatedFiles);

			return 0;
		}

		private static void generateCommonModule(string outDir, HashSet<string> generatedFiles)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"TSClientGen." + CommonModuleName))
			using (var streamReader = new StreamReader(stream))
			{
				File.WriteAllText(Path.Combine(outDir, CommonModuleName), streamReader.ReadToEnd());
			}
			generatedFiles.Add(Path.Combine(outDir, CommonModuleName).ToLowerInvariant());			
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

					imports.AppendLine($" }} from './enums/{enums.Key}'");
				}

				string moduleFileName = Path.Combine(outDir, $"{enumsByModule.Key}.ts");
				if (!File.Exists(moduleFileName))
					moduleFileName = Path.Combine(outDir, $"{enumsByModule.Key}.d.ts");
				File.AppendAllText(moduleFileName, imports.ToString());
				imports.Clear();
			}
		}

		private static void generateEnumsDefinition(
			EnumMapper enumMapper,
			IReadOnlyCollection<TSExtendEnumAttribute> staticMemberProviders,
			string outDir,
			string[] localizationLanguages,
			string defaultLocale,
			HashSet<string> generatedFiles)
		{
			var sw = new Stopwatch();
			sw.Start();

			var typeMapper = new TypeMapper();
			var enumsDefinition = new StringBuilder(2048);
			var staticMemberProvidersLookup = staticMemberProviders.ToLookup(e => e.EnumType);

			var enumsOutDir = Path.Combine(outDir, "enums");
			if (!Directory.Exists(enumsOutDir))
				Directory.CreateDirectory(enumsOutDir);

			foreach (var enums in enumMapper.GetEnumsByModules())
			{
				enumsDefinition.GenerateEnums(enums, staticMemberProvidersLookup, typeMapper, defaultLocale);
				string targetFileName = $"{enums.Key}.ts";
				File.WriteAllText(Path.Combine(enumsOutDir, targetFileName), enumsDefinition.ToString());
				enumsDefinition.Clear();
				generatedFiles.Add(Path.Combine(enumsOutDir, targetFileName).ToLowerInvariant());
				fixFilenameCase(enumsOutDir, targetFileName);
			}

			foreach (var culture in localizationLanguages)
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
				string targetFileName = (culture == defaultLocale) ? "enums.resx" : $"enums.{culture}.resx";
				using (var enumResxFileWriter = new ResXResourceWriter(Path.Combine(outDir, targetFileName)))
				{
					enumResxFileWriter.GenerateEnumLocalizations(
						staticMemberProviders.OfType<TSEnumLocalizationAttribute>().ToList());
				}
				fixFilenameCase(outDir, targetFileName);
				generatedFiles.Add(Path.Combine(outDir, targetFileName).ToLowerInvariant());
			}

			Console.WriteLine($"Enums generated in {sw.ElapsedMilliseconds} ms");
		}

		private static void generateResources(
			Assembly targetAsm,
			string outDir,
			string[] localizationLanguages,
			string defaultLocale,
			HashSet<string> generatedFiles)
		{
			var resources = targetAsm.GetCustomAttributes().OfType<TSExposeResxAttribute>().ToList();
			foreach (var culture in localizationLanguages)
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

				foreach (var resource in resources)
				{
					var targetFileName = (culture == defaultLocale)
						? $"{resource.ResxName}.resx"
						: $"{resource.ResxName}.{culture}.resx";
					using (var resxFileWriter = new ResXResourceWriter(Path.Combine(outDir, targetFileName)))
					{
						var resourceSet = resource.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
						foreach (DictionaryEntry entry in resourceSet)
						{
							resxFileWriter.AddResource(entry.Key.ToString(), entry.Value);
						}
					}
					fixFilenameCase(outDir, targetFileName);
					generatedFiles.Add(Path.Combine(outDir, targetFileName).ToLowerInvariant());
				}

			}
		}

		private static void generateClientsFromAsm(Assembly targetAsm, string outDir, EnumMapper enumMapper, HashSet<string> generatedFiles)
		{
			var sw = new Stopwatch();
			sw.Start();

			var moduleNames = new HashSet<string>();

			var controllers = targetAsm.GetTypes().Where(t => typeof(IHttpController).IsAssignableFrom(t));

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

				var result = new StringBuilder(2048);
				var controllerRoute = controller.GetCustomAttributes<System.Web.Http.RouteAttribute>().SingleOrDefault();
				result.GenerateControllerClient(controller, mapper, controllerRoute);

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
								result.GenerateTypeDefinition(typeDescriptor, mapper);
							}
							else
							{
								result.GenerateInterface(typeDescriptor, mapper);
							}
						}
						generatedTypes.Add(typeDescriptor.Type);
					}
				} while (typesToGenerate.Any());

				TSStaticContentAttribute staticContentModule;
				if (staticContentByModuleName.TryGetValue(moduleName, out staticContentModule))
				{
					result.GenerateStaticContent(staticContentModule);
				}

				string targetFileName = $"{moduleName}.ts";
				File.WriteAllText(Path.Combine(outDir, targetFileName), result.ToString());
				fixFilenameCase(outDir, targetFileName);
				generatedFiles.Add(Path.Combine(outDir, targetFileName).ToLowerInvariant());

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
				string targetFileName = $"{staticContent.ModuleName}.ts";
				File.WriteAllText(Path.Combine(outDir, targetFileName), result.ToString());
				fixFilenameCase(outDir, targetFileName);
				generatedFiles.Add(Path.Combine(outDir, targetFileName).ToLowerInvariant());
			}
		}

		private static void fixFilenameCase(string outDir, string targetFileName)
		{
			var existingFileName = Directory.EnumerateFiles(outDir, targetFileName).Single();
			if (targetFileName != Path.GetFileName(existingFileName))
			{
				// отличаются регистром, надо переименовать
				new FileInfo(existingFileName).MoveTo(Path.Combine(outDir, targetFileName));
			}
		}

		private static void cleanupOutDir(string outDir, HashSet<string> generatedFiles)
		{
			var filesToDelete = Directory
				.EnumerateFiles(outDir, "*.*", SearchOption.AllDirectories)
				.Where(file => !generatedFiles.Contains(file.ToLowerInvariant()))
				.ToList();

			if (!filesToDelete.Any())
				return;

			Console.WriteLine("Cleaning up...");
			foreach (var existingFile in filesToDelete)
			{
				File.Delete(existingFile);
				Console.WriteLine($"\t{existingFile} deleted");
			}			
		}
	}
}