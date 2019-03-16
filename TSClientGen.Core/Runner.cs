using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TSClientGen.ApiDescriptors;

namespace TSClientGen
{
	public class Runner
	{
		public Runner(
			IArguments arguments,
			IApiDiscovery apiDiscovery,
			ICustomTypeConverter customTypeConverter,
			IPropertyNameProvider propertyNameProvider,
			IResourceModuleWriterFactory resourceModuleWriterFactory,
			Func<object, string> serializeToJson)
		{
			_arguments = arguments;
			_apiDiscovery = apiDiscovery;
			_customTypeConverter = customTypeConverter;
			_propertyNameProvider = propertyNameProvider;
			_resourceModuleWriterFactory = resourceModuleWriterFactory;
			_serializeToJson = serializeToJson;
		}
		
		
		public void Execute()
		{
			if (!Directory.Exists(_arguments.OutputPath))
				Directory.CreateDirectory(_arguments.OutputPath);

			var moduleNamesByAssemblyPath = _arguments.AssemblyNames
				.Zip(_arguments.AssemblyPaths, (name, path) => new { name, path })
				.ToDictionary(v => v.path, v => v.name);
			var enumMapper = new EnumMapper(moduleNamesByAssemblyPath);
			var enumStaticMemberProviders = new List<TSExtendEnumAttribute>();

			var generatedFiles = new HashSet<string>();

			if (_arguments.CommonModuleName == null)
			{
				ApiClientModuleGenerator.WriteDefaultCommonModule(_arguments.OutputPath, generatedFiles);
			}

			foreach (var asmPath in _arguments.AssemblyPaths)
			{
				var asm = Assembly.LoadFrom(asmPath);
				generateClientsFromAsm(asm, _arguments.OutputPath, enumMapper, generatedFiles);				
				generateResources(
					asm,
					_arguments.OutputPath,
					_arguments.LocalizationLanguages,
					_arguments.DefaultLocale,
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

			appendEnumImports(enumMapper, _arguments.OutputPath);

			generateEnumsDefinition(
				enumMapper,
				enumStaticMemberProviders,
				_arguments.OutputPath,
				_arguments.LocalizationLanguages,
				_arguments.DefaultLocale,
				generatedFiles);

			cleanupOutDir(_arguments.OutputPath, generatedFiles);			
		}
				
		private void generateClientsFromAsm(Assembly assembly, string outDir, EnumMapper enumMapper, HashSet<string> generatedFiles)
		{
			var sw = new Stopwatch();
			sw.Start();

			var moduleNames = new HashSet<string>();

			var staticContentByModuleName = assembly.GetCustomAttributes()
				.OfType<TSStaticContentAttribute>()
				.ToDictionary(attr => attr.ModuleName);
			
			// Генерация клиентов к контроллерам
			foreach (var module in _apiDiscovery.GetModules(assembly, enumMapper, _customTypeConverter))
			{
				if (moduleNames.Contains(module.Name))
					throw new Exception("Duplicate module name - " + module.Name);
				moduleNames.Add(module.Name);

				var generator = new ApiClientModuleGenerator(module, _propertyNameProvider, _serializeToJson, _arguments.CommonModuleName);
				generator.WriteApiClient();

				var typesToGenerate = new HashSet<Type>();
				var generatedTypes = new HashSet<Type>();

				do
				{
					var knownTypes = module.TypeMapping.GetCustomTypes();
					typesToGenerate.UnionWith(knownTypes);
					typesToGenerate.ExceptWith(generatedTypes);
					
					foreach (var typeDescriptor in typesToGenerate.Select(t => module.TypeMapping.GetDescriptorByType(t)))
					{
						if (typeDescriptor.Type.IsEnum)
						{
							enumMapper.SaveEnum(assembly, module.Name, typeDescriptor.Type);
						}
						else
						{
							switch (typeDescriptor)
							{
								case CustomTypeDescriptor typeDesc:
									generator.WriteType(typeDesc);
									break;
								case InterfaceDescriptor interfaceDesc:
									generator.WriteInterface(interfaceDesc);
									break;
								default:
									throw new Exception("Unexpected type descriptor - " + typeDescriptor.GetType().FullName);
							}
						}
						generatedTypes.Add(typeDescriptor.Type);
					}
				} while (typesToGenerate.Any());

				if (staticContentByModuleName.TryGetValue(module.Name, out var staticContentModule))
				{
					generator.WriteStaticContent(staticContentModule);
				}

				string targetFileName = $"{module.Name}.ts";
				File.WriteAllText(Path.Combine(outDir, targetFileName), generator.GetResult());
				fixFilenameCase(outDir, targetFileName);
				generatedFiles.Add(Path.Combine(outDir, targetFileName).ToLowerInvariant());

				Console.WriteLine($"TypeScript client `{module.Name}` generated in {sw.ElapsedMilliseconds} ms");
				sw.Restart();
			}

			foreach (var staticContent in staticContentByModuleName.Values.Where(attr => !moduleNames.Contains(attr.ModuleName)))
			{
				if (moduleNames.Contains(staticContent.ModuleName))
					throw new Exception("Duplicate module name - " + staticContent.ModuleName);

				moduleNames.Add(staticContent.ModuleName);

				string targetFileName = $"{staticContent.ModuleName}.ts";
				File.WriteAllLines(
					Path.Combine(outDir, targetFileName),
					staticContent.Content.Select(entry => $"export let {entry.Key} = {_serializeToJson(entry.Value)};"));
				fixFilenameCase(outDir, targetFileName);
				generatedFiles.Add(Path.Combine(outDir, targetFileName).ToLowerInvariant());
			}
		}
		
		private void appendEnumImports(EnumMapper enumMapper, string outDir)
		{
			var mapper = new TypeMapping(_customTypeConverter);
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

		private void generateEnumsDefinition(
			EnumMapper enumMapper,
			IReadOnlyCollection<TSExtendEnumAttribute> staticMemberProviders,
			string outDir,
			string[] localizationLanguages,
			string defaultLocale,
			HashSet<string> generatedFiles)
		{
			var sw = new Stopwatch();
			sw.Start();

			var typeMapper = new TypeMapping(_customTypeConverter);
			var staticMemberProvidersLookup = staticMemberProviders.ToLookup(e => e.EnumType);

			var enumsOutDir = Path.Combine(outDir, "enums");
			if (!Directory.Exists(enumsOutDir))
				Directory.CreateDirectory(enumsOutDir);

			foreach (var enums in enumMapper.GetEnumsByModules())
			{
				var enumModuleGenerator = new EnumModuleGenerator();
				enumModuleGenerator.Write(enums, staticMemberProvidersLookup, typeMapper, defaultLocale);

				string targetFileName = $"{enums.Key}.ts";
				File.WriteAllText(Path.Combine(enumsOutDir, targetFileName), enumModuleGenerator.GetResult());
				generatedFiles.Add(Path.Combine(enumsOutDir, targetFileName).ToLowerInvariant());
				fixFilenameCase(enumsOutDir, targetFileName);
			}

			var enumLocalizationAttributes = staticMemberProviders.OfType<TSEnumLocalizationAttribute>().ToList();
			if (_resourceModuleWriterFactory != null && enumLocalizationAttributes.Any())
			{
				foreach (var culture in localizationLanguages)
				{
					string targetFileName;
					using (var resourceModuleWriter = _resourceModuleWriterFactory.Create(outDir, "enums", culture, defaultLocale))
					{
						targetFileName = resourceModuleWriter.Filename;
						var enumModuleGenerator = new ResourceModuleGenerator(resourceModuleWriter);
						enumModuleGenerator.WriteEnumLocalizations(enumLocalizationAttributes);
					}
					fixFilenameCase(outDir, targetFileName);
					generatedFiles.Add(Path.Combine(outDir, targetFileName).ToLowerInvariant());
				}
			}

			Console.WriteLine($"Enums generated in {sw.ElapsedMilliseconds} ms");
		}

		private void generateResources(
			Assembly targetAsm,
			string outDir,
			string[] localizationLanguages,
			string defaultLocale,
			HashSet<string> generatedFiles)
		{
			var resources = targetAsm.GetCustomAttributes().OfType<TSExposeResxAttribute>().ToList();
			if (_resourceModuleWriterFactory == null || !resources.Any())
				return;
			
			foreach (var culture in localizationLanguages)
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

				foreach (var resource in resources)
				{
					string targetFilename;
					using (var resourceWriter = _resourceModuleWriterFactory.Create(
						outDir, resource.ResxName, culture, defaultLocale))
					{
						targetFilename = resourceWriter.Filename;
						var resourceSet = resource.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
						foreach (DictionaryEntry entry in resourceSet)
						{
							resourceWriter.AddResource(entry.Key.ToString(), entry.Value.ToString());
						}
					}
					fixFilenameCase(outDir, targetFilename);
					generatedFiles.Add(Path.Combine(outDir, targetFilename).ToLowerInvariant());
				}
			}
		}
		
		private static void fixFilenameCase(string outDir, string targetFileName)
		{
			var existingFileName = Directory.EnumerateFiles(outDir, targetFileName).Single();
			if (targetFileName != Path.GetFileName(existingFileName))
			{
				// filenames differ in case, renaming
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
		
		
		private readonly IArguments _arguments;
		private readonly IApiDiscovery _apiDiscovery;
		private readonly ICustomTypeConverter _customTypeConverter;
		private readonly IPropertyNameProvider _propertyNameProvider;
		private readonly IResourceModuleWriterFactory _resourceModuleWriterFactory;
		private readonly Func<object, string> _serializeToJson;
	}
}