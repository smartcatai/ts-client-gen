using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class Runner
	{
		public Runner(
			Arguments arguments,
			IApiDiscovery apiDiscovery,
			ITypeConverter customTypeConverter,
			IEnumerable<ITypeDescriptorProvider> typeDescriptorProviders,
			IApiClientWriter customApiClientWriter,
			IResultFileWriter resultFileWriter,
			IImportEnumModuleGenerator importEnumModuleGenerator,
			Func<object, string> serializeToJson)
		{
			_arguments = arguments;
			_apiDiscovery = apiDiscovery;
			_customTypeConverter = customTypeConverter;
			_typeDescriptorProviders = typeDescriptorProviders;
			_customApiClientWriter = customApiClientWriter;
			_resultFileWriter = resultFileWriter;
			_importEnumModuleGenerator = importEnumModuleGenerator;
			_serializeToJson = serializeToJson;
		}

		public static Arguments ParseArguments(string[] args)
		{
			var argsParseResult = Parser.Default.ParseArguments<Arguments>(args);
			if (argsParseResult is NotParsed<Arguments> notParsed)
			{
				foreach (var error in notParsed.Errors)
				{
					Console.WriteLine(error.ToString());
				}
				return null;
			}

			var arguments = ((Parsed<Arguments>) argsParseResult).Value;
			if (arguments.BuiltinTransportModule == null && arguments.CustomTransportModule == null)
			{
				Console.WriteLine("Specify either --transport or --custom-transport command-line option");
				return null;
			}

			return arguments;
		}

		public static InjectedPlugin LoadPlugin(Arguments arguments)
		{
			var plugins = new InjectedPlugin();
			if (arguments.PluginsAssembly != null)
			{
				var pluginsAssembly = Assembly.LoadFrom(arguments.PluginsAssembly);
				using (var compositionContainer = new CompositionContainer(new AssemblyCatalog(pluginsAssembly)))
				{
					compositionContainer.ComposeParts(plugins);
				}
			}
			return plugins;
		}

		public void Execute()
		{
			string enumsModuleName = _arguments.EnumsModuleName ?? "enums";

			_resultFileWriter.WriteBuiltinModule(ApiModuleGenerator.TransportContractsModuleName + ".ts");
			string builtinTransportModule = (_arguments.BuiltinTransportModule != null)
				? $"transport-{_arguments.BuiltinTransportModule.ToString().ToLower()}.ts"
				: null;
			if (builtinTransportModule != null)
			{
				_resultFileWriter.WriteBuiltinModule(builtinTransportModule);
			}

			var allEnums = new HashSet<Type>();
			var assemblies = _arguments.AssemblyPaths.Select(Assembly.LoadFrom).ToList();
			foreach (var assembly in assemblies)
			{
				var transportModuleName = _arguments.CustomTransportModule ?? $"./{builtinTransportModule}";
				if (transportModuleName.EndsWith(".ts", StringComparison.InvariantCultureIgnoreCase))
					transportModuleName = transportModuleName.Remove(transportModuleName.Length - 3);

				var apiClientModules = _apiDiscovery.GetModules(assembly);
				var staticContentModules = assembly.GetCustomAttributes().OfType<TSStaticContentAttribute>();
				GenerateApiClients(apiClientModules, staticContentModules, transportModuleName, enumsModuleName, allEnums);

				var resources = assembly.GetCustomAttributes().OfType<TSExposeResxAttribute>().ToList();
				GenerateResources(resources);
			}

			var staticMemberGeneratorsByEnumType = CollectEnumStaticMemberGenerators(assemblies, allEnums);
			var enumLocalizationsByEnumType = CollectEnumLocalizationAttributes(assemblies, allEnums);
			if (allEnums.Any())
			{
				GenerateEnumsModule(allEnums, enumsModuleName, staticMemberGeneratorsByEnumType, enumLocalizationsByEnumType);
			}

			if (_arguments.CleanupOutDir)
			{
				_resultFileWriter.CleanupOutDir();
			}
		}

		public void GenerateApiClients(
			IEnumerable<ApiClientModule> apiClientModules,
			IEnumerable<TSStaticContentAttribute> staticContentModules,
			string transportModuleName,
			string enumsModuleName,
			HashSet<Type> allEnums)
		{
			var sw = new Stopwatch();
			sw.Start();

			var moduleNames = new HashSet<string>();

			var staticContentByModuleName = staticContentModules.ToDictionary(attr => attr.ModuleName);

			// generating client modules for api controllers
			foreach (var module in apiClientModules)
			{
				if (moduleNames.Contains(module.Name))
					throw new Exception("Duplicate module name - " + module.Name);
				moduleNames.Add(module.Name);

				var typeMapping = new TypeMapping(
					_customTypeConverter,
					_typeDescriptorProviders,
					_arguments.AppendIPrefix,
					TypeMappingConfig.FromArgs(_arguments));
				foreach (var type in module.ExplicitlyRequiredTypes.Where(t => !t.IsEnum))
				{
					typeMapping.AddType(type);
				}

				var generator = new ApiModuleGenerator(module, typeMapping, _customApiClientWriter, _serializeToJson, transportModuleName);
				generator.WriteApiClientClass();
				generator.WriteTypeDefinitions();
				generator.WriteEnumImports(enumsModuleName);

				var explictlyRequiredEnums = module.ExplicitlyRequiredTypes.Where(t => t.IsEnum);
				foreach (var enumType in typeMapping.GetEnums().Union(explictlyRequiredEnums))
				{
					allEnums.Add(enumType);
				}

				if (staticContentByModuleName.TryGetValue(module.Name, out var staticContentModule))
				{
					generator.WriteStaticContent(staticContentModule);
				}

				_resultFileWriter.WriteFile($"{module.Name}.ts", generator.GetResult());

				Console.WriteLine($"TypeScript client `{module.Name}` generated in {sw.ElapsedMilliseconds} ms");
				sw.Restart();
			}

			foreach (var staticContent in staticContentByModuleName.Values.Where(attr => !moduleNames.Contains(attr.ModuleName)))
			{
				if (moduleNames.Contains(staticContent.ModuleName))
					throw new Exception("Duplicate module name - " + staticContent.ModuleName);

				moduleNames.Add(staticContent.ModuleName);

				var content = string.Join("\r\n",
					staticContent.Content.Select(entry =>
						$"export let {entry.Key} = {_serializeToJson(entry.Value)};"));
				_resultFileWriter.WriteFile($"{staticContent.ModuleName}.ts", content);
			}
		}

		public Dictionary<Type, List<Func<string>>> CollectEnumStaticMemberGenerators(
			IReadOnlyCollection<Assembly> assemblies,
			HashSet<Type> allEnums)
		{
			var generatorsByEnumType = new Dictionary<Type, List<Func<string>>>();

			foreach (var attr in assemblies.SelectMany(
				asm => asm.GetCustomAttributes().OfType<ForAssembly.TSExtendEnumAttribute>()))
			{
				if (!generatorsByEnumType.TryGetValue(attr.EnumType, out var list))
				{
					list = generatorsByEnumType[attr.EnumType] = new List<Func<string>>();
				}
				list.Add(attr.GenerateStaticMembers);
				allEnums.Add(attr.EnumType);
			}

			foreach (var @enum in allEnums)
			{
				foreach (var attr in @enum.GetCustomAttributes().OfType<TSExtendEnumAttribute>())
				{
					if (!generatorsByEnumType.TryGetValue(@enum, out var list))
					{
						list = generatorsByEnumType[@enum] = new List<Func<string>>();
					}
					list.Add(attr.GenerateStaticMembers);
				}
			}

			return generatorsByEnumType;
		}

		public Dictionary<Type, TSEnumLocalizationAttributeBase> CollectEnumLocalizationAttributes(
			IReadOnlyCollection<Assembly> assemblies,
			HashSet<Type> allEnums)
		{
			var attributesByEnumType = new Dictionary<Type, TSEnumLocalizationAttributeBase>();

			foreach (var attr in assemblies.SelectMany(
				asm => asm.GetCustomAttributes().OfType<ForAssembly.TSEnumLocalizationAttribute>()))
			{
				if (attributesByEnumType.ContainsKey(attr.EnumType))
				{
					throw new InvalidOperationException($"TSEnumLocalizationAttribute has been applied more than once for the enum {attr.EnumType.FullName}");
				}
				attributesByEnumType[attr.EnumType] = attr;
				allEnums.Add(attr.EnumType);
			}

			foreach (var enumType in allEnums)
			{
				var attr = enumType.GetCustomAttribute<TSEnumLocalizationAttribute>();
				if (attr != null)
				{
					if (attributesByEnumType.ContainsKey(enumType))
					{
						throw new InvalidOperationException($"TSEnumLocalizationAttribute has been applied more than once for the enum {enumType.FullName}");
					}
					attributesByEnumType[enumType] = attr;
				}
			}

			return attributesByEnumType;
		}

		public void GenerateEnumsModule(
			IReadOnlyCollection<Type> enums,
			string enumsModuleName,
			Dictionary<Type, List<Func<string>>> staticMemberGeneratorsByEnumType,
			Dictionary<Type, TSEnumLocalizationAttributeBase> localizationByEnumType)
		{
			var sw = new Stopwatch();
			sw.Start();

			var duplicateEnumNames = enums
				.GroupBy(e => e.Name)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();
			if (duplicateEnumNames.Any())
			{
				throw new InvalidOperationException(
					$"Duplicate enum names found - {string.Join(", ", duplicateEnumNames)}");
			}

			if (enumsModuleName.EndsWith(".ts"))
				enumsModuleName = enumsModuleName.Remove(enumsModuleName.Length - 3);

			foreach (var enumType in enums)
			{
				var enumModuleGenerator = new EnumModuleGenerator();
				enumModuleGenerator.Write(
					enumType,
					_arguments.UseStringEnums,
					_arguments.GetResourceModuleName,
					staticMemberGeneratorsByEnumType.TryGetValue(enumType, out var generator) ? generator : null,
					localizationByEnumType.TryGetValue(enumType, out var localization) ? localization : null
				);

				var filePath = Path.Combine(enumsModuleName, enumType.Name, "index.ts");

				_resultFileWriter.WriteFile(filePath, enumModuleGenerator.GetResult());
			}

			var importEnumModule = _importEnumModuleGenerator.Generate(enums, enumsModuleName);
			_resultFileWriter.WriteFile($"{enumsModuleName}.ts", importEnumModule);

			if (localizationByEnumType.Any())
			{
				if (!_resultFileWriter.CanWriteResourceFiles)
				{
					throw new InvalidOperationException(
						"TSEnumLocalization attributes found in processed assemblies, " +
						"but no IResourceModuleWriterFactory instance has been provided via a plugin"
					);
				}

				foreach (var kvp in localizationByEnumType)
				{
					var (enumType, localization) = (kvp.Key, kvp.Value);
					foreach (var culture in _arguments.LocalizationLanguages)
					{
						var folderPath = Path.Combine(enumsModuleName, enumType.Name);
						using var resourceModuleWriter = _resultFileWriter.WriteResourceFile(folderPath, culture);
						var enumResourceModuleGenerator = new EnumResourceModuleGenerator(resourceModuleWriter);
						enumResourceModuleGenerator.WriteEnumLocalizations(enumType, localization);
					}
				}
			}

			Console.WriteLine($"Enums generated in {sw.ElapsedMilliseconds} ms");
		}

		public void GenerateResources(IReadOnlyList<TSExposeResxAttribute> resources)
		{
			if (!resources.Any())
				return;

			if (!_resultFileWriter.CanWriteResourceFiles)
			{
				throw new InvalidOperationException(
					"TSExposeResx attributes found in processed assemblies, but no IResourceModuleWriterFactory instance has been provided via a plugin");
			}

			foreach (var culture in _arguments.LocalizationLanguages)
			{
				var cultureInfo = CultureInfo.GetCultureInfo(culture);
				foreach (var resource in resources)
				{
					using (var resourceWriter = _resultFileWriter.WriteResourceFile(resource.ResxName, culture))
					{
						var resourceSet = resource.ResourceManager.GetResourceSet(cultureInfo, true, true);
						foreach (DictionaryEntry entry in resourceSet)
						{
							resourceWriter.AddResource(entry.Key.ToString(), entry.Value.ToString());
						}
					}
				}
			}
		}


		private readonly Arguments _arguments;
		private readonly IApiDiscovery _apiDiscovery;
		private readonly ITypeConverter _customTypeConverter;
		private readonly IEnumerable<ITypeDescriptorProvider> _typeDescriptorProviders;
		private readonly IApiClientWriter _customApiClientWriter;
		private readonly IResultFileWriter _resultFileWriter;
		private readonly IImportEnumModuleGenerator _importEnumModuleGenerator;
		private readonly Func<object, string> _serializeToJson;
	}
}
