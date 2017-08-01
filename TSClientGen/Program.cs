using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
			var enumStaticMemberProviders = new List<TypeScriptExtendEnumAttribute>();

			foreach (var asmPath in arguments.AssembliesPath)
			{
				var asm = Assembly.LoadFrom(asmPath);
				generateClientsFromAsm(asm, arguments.ControllerClientsOutputDirPath, enumMapper);

				foreach (var attr in asm.GetCustomAttributes().OfType<TypeScriptExtendEnumAttribute>())
				{
					enumStaticMemberProviders.Add(attr);
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

		private static void generateEnumsDefinition(EnumMapper enumMapper, IReadOnlyCollection<TypeScriptExtendEnumAttribute> staticMemberProviders, Arguments args)
		{
			var sw = new Stopwatch();
			sw.Start();

			var typeMapper = new TypeMapper();
			var enumsDefinition = new StringBuilder(2048);
			var staticMemberProvidersLookup = staticMemberProviders.ToLookup(e => e.EnumType);

			foreach (var enums in enumMapper.GetEnumsByModules())
			{
				enumsDefinition.GenerateEnums(enums, staticMemberProvidersLookup, typeMapper);
				File.WriteAllText(Path.Combine(args.EnumsOutputDirPath, $"{enums.Key}.ts"), enumsDefinition.ToString());
				enumsDefinition.Clear();
			}

			foreach (var culture in args.LocalizationLanguages)
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
				var enumPoFile = new StringBuilder(2048);
				enumPoFile.GenerateEnumLocalizations(staticMemberProviders.OfType<TypeScriptEnumLocalizationAttribute>().ToList());
				File.WriteAllText(Path.Combine(args.ResourcesOutputDirPath, $"enums.{culture}.po"), enumPoFile.ToString());
			}

			Console.WriteLine($"Enums generated in {sw.ElapsedMilliseconds} ms");
		}

		private static void generateClientsFromAsm(Assembly targetAsm, string outDir, EnumMapper enumMapper)
		{
			var sw = new Stopwatch();
			sw.Start();

			var moduleNames = new HashSet<string>();

			var controllers = targetAsm.GetTypes()
				.Where(t => typeof(IHttpController).IsAssignableFrom(t) || typeof(IController).IsAssignableFrom(t));

			var staticContentByModuleName = targetAsm.GetCustomAttributes().OfType<TypeScriptStaticContentAttribute>()
				.ToDictionary(attr => attr.ModuleName);

			// Генерация клиентов к контроллерам
			foreach (var controller in controllers)
			{
				var tsModuleAttribute = controller.GetCustomAttributes<TypeScriptModuleAttribute>().SingleOrDefault();
				var moduleName = tsModuleAttribute?.ModuleName;
				if (moduleName != null)
				{
					if (moduleNames.Contains(moduleName))
						throw new Exception("Duplicate module name - " + moduleName);
					moduleNames.Add(moduleName);
				}

				var mapper = new TypeMapper();

				var additionalTypes = controller.GetCustomAttributes<TypeScriptTypeAttribute>();
				foreach (var typeAttribute in additionalTypes)
				{
					if (typeAttribute.SubstituteType == null)
						throw new Exception("TypeScriptType attribute with string type declaration is not allowed on controlller");

					if (typeAttribute.SubstituteType.IsEnum)
					{
						enumMapper.SaveEnum(targetAsm, typeAttribute.SubstituteType);
					}
					else
					{
						if (moduleName == null || !typeof(IHttpController).IsAssignableFrom(controller))
							throw new Exception($"Controller {controller.FullName} has invalid TypeScriptType attributes. Only enum types can be specified for MVC controllers or API controllers without TypeScriptModule attribute.");

						mapper.AddType(typeAttribute.SubstituteType);
					}
				}

				if (moduleName == null)
					continue;

				var result = new StringBuilder(2048);
				var controllerRoute = controller.GetCustomAttributes<System.Web.Http.RouteAttribute>().SingleOrDefault();
				if (tsModuleAttribute.LoadedAsJsonModule)
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

					foreach (var type in typesToGenerate)
					{
						if (type.IsEnum)
						{
							enumMapper.SaveEnum(targetAsm, moduleName, type);
						}
						else
						{
							var tsTypeAttribute = type.GetCustomAttributes<TypeScriptTypeAttribute>().FirstOrDefault();
							if (tsTypeAttribute != null && tsTypeAttribute.TypeDefinition != null)
							{
								result.GenerateTypeDefinition(type, tsTypeAttribute.TypeDefinition, mapper, !tsModuleAttribute.LoadedAsJsonModule);
							}
							else
							{
								result.GenerateInterface(type, mapper, !tsModuleAttribute.LoadedAsJsonModule);
							}
						}
						generatedTypes.Add(type);
					}
				} while (typesToGenerate.Any());

				if (!tsModuleAttribute.LoadedAsJsonModule)
				{
					TypeScriptStaticContentAttribute staticContentModule;
					if (staticContentByModuleName.TryGetValue(moduleName, out staticContentModule))
					{
						result.GenerateStaticContent(staticContentModule);
					}
				}

				string extension = tsModuleAttribute.LoadedAsJsonModule ? "d.ts" : "ts";
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
