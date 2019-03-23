using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TSClientGen.Extensibility;

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
		
		
		public const string DefaultCommonModuleName = "common.ts";		
		
		
		public void Execute()
		{
			if (!Directory.Exists(_arguments.OutDir))
				Directory.CreateDirectory(_arguments.OutDir);

			string enumsModuleName = _arguments.EnumsModuleName ?? "enums";
			var enumStaticMemberProviders = new List<TSExtendEnumAttribute>();

			_generatedFiles = new HashSet<string>();

			var commonModuleName = _arguments.CommonModuleName ?? "./" + DefaultCommonModuleName;
			if (_arguments.CommonModuleName == null)
			{
				writeDefaultCommonModule();
			}			

			var allEnums = new HashSet<Type>();
			foreach (var asmPath in _arguments.AssemblyPaths)
			{
				var asm = Assembly.LoadFrom(asmPath);
				generateClientsFromAsm(asm, commonModuleName, enumsModuleName, allEnums);				
				generateResources(asm);

				foreach (var attr in asm.GetCustomAttributes().OfType<TSExtendEnumAttribute>())
				{
					enumStaticMemberProviders.Add(attr);
					allEnums.Add(attr.EnumType);
				}
			}

			generateEnumsModule(allEnums, enumStaticMemberProviders, enumsModuleName);

			if (_arguments.CleanupOutDir)
			{
				cleanupOutDir();
			}
		}

		private void writeDefaultCommonModule()
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"TSClientGen." + DefaultCommonModuleName))
			using (var streamReader = new StreamReader(stream))
			{
				File.WriteAllText(
					Path.Combine(_arguments.OutDir, DefaultCommonModuleName), 
					streamReader.ReadToEnd());
			}
			_generatedFiles.Add(Path.Combine(_arguments.OutDir, DefaultCommonModuleName).ToLowerInvariant());			
		}
		
		private void generateClientsFromAsm(Assembly assembly, string commonModuleName, string enumsModuleName, HashSet<Type> allEnums)
		{
			var sw = new Stopwatch();
			sw.Start();

			var moduleNames = new HashSet<string>();

			var staticContentByModuleName = assembly.GetCustomAttributes()
				.OfType<TSStaticContentAttribute>()
				.ToDictionary(attr => attr.ModuleName);
			
			// generating client modules for api controllers
			var modules = _apiDiscovery.GetModules(
				assembly, 
				t => t.GetCustomAttribute<TSModuleAttribute>() != null);
			foreach (var module in modules)
			{
				if (moduleNames.Contains(module.Name))
					throw new Exception("Duplicate module name - " + module.Name);
				moduleNames.Add(module.Name);

				var typeMapping = new TypeMapping(_customTypeConverter);
				foreach (var type in module.AdditionalTypes)
				{
					typeMapping.AddType(type);
				}
				
				var generator = new ApiModuleGenerator(module, typeMapping, _propertyNameProvider, _serializeToJson, commonModuleName);
				generator.WriteApiClientClass();
				generator.WriteTypeDefinitions();
				generator.WriteEnumImports(enumsModuleName);

				foreach (var enumType in typeMapping.GetTypesToGenerate().Where(t => t.IsEnum).ToList())
				{
					allEnums.Add(enumType);
				}

				if (staticContentByModuleName.TryGetValue(module.Name, out var staticContentModule))
				{
					generator.WriteStaticContent(staticContentModule);
				}

				writeFile($"{module.Name}.ts", generator.GetResult());

				Console.WriteLine($"TypeScript client `{module.Name}` generated in {sw.ElapsedMilliseconds} ms");
				sw.Restart();
			}

			foreach (var staticContent in staticContentByModuleName.Values.Where(attr => !moduleNames.Contains(attr.ModuleName)))
			{
				if (moduleNames.Contains(staticContent.ModuleName))
					throw new Exception("Duplicate module name - " + staticContent.ModuleName);

				moduleNames.Add(staticContent.ModuleName);

				var content = string.Join(Environment.NewLine,
					staticContent.Content.Select(entry =>
						$"export let {entry.Key} = {_serializeToJson(entry.Value)};"));
				writeFile($"{staticContent.ModuleName}.ts", content);
			}
		}

		private void generateEnumsModule(
			IEnumerable<Type> enums,
			IReadOnlyCollection<TSExtendEnumAttribute> staticMemberProviders,
			string enumsModuleName)
		{
			var sw = new Stopwatch();
			sw.Start();

			var staticMemberProvidersLookup = staticMemberProviders.ToLookup(e => e.EnumType);

			if (!enumsModuleName.EndsWith(".ts"))
				enumsModuleName += ".ts";
			
			var enumModuleGenerator = new EnumModuleGenerator();
			enumModuleGenerator.Write(enums, _arguments.GetResourceModuleName, staticMemberProvidersLookup);
			writeFile(enumsModuleName, enumModuleGenerator.GetResult());

			var enumLocalizationAttributes = staticMemberProviders.OfType<TSEnumLocalizationAttribute>().ToList();
			if (_resourceModuleWriterFactory != null && enumLocalizationAttributes.Any())
			{
				foreach (var culture in _arguments.LocalizationLanguages)
				{
					using (var resourceModuleWriter = writeResourceFile(enumsModuleName, culture))
					{
						var enumResourceModuleGenerator = new EnumResourceModuleGenerator(resourceModuleWriter);
						enumResourceModuleGenerator.WriteEnumLocalizations(enumLocalizationAttributes);
					}
				}
			}
			
			Console.WriteLine($"Enums generated in {sw.ElapsedMilliseconds} ms");
		}

		private void generateResources(Assembly targetAsm)
		{
			var resources = targetAsm.GetCustomAttributes().OfType<TSExposeResxAttribute>().ToList();
			if (_resourceModuleWriterFactory == null || !resources.Any())
				return;
			
			foreach (var culture in _arguments.LocalizationLanguages)
			{
				var cultureInfo = CultureInfo.GetCultureInfo(culture);
				foreach (var resource in resources)
				{
					using (var resourceWriter = writeResourceFile(resource.ResxName, culture))
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

		private void writeFile(string filename, string text)
		{
			File.WriteAllText(Path.Combine(_arguments.OutDir, filename), text);
			fixFilenameCase(filename);
			_generatedFiles.Add(Path.Combine(_arguments.OutDir, filename).ToLowerInvariant());
		}

		private IResourceModuleWriter writeResourceFile(string filename, string culture)
		{
			var moduleWriter = _resourceModuleWriterFactory.Create(
				_arguments.OutDir, filename, culture, _arguments.DefaultLocale);
			return new ResourceFileWriter(moduleWriter, () =>
			{
				var fullFilename = Path.Combine(_arguments.OutDir, moduleWriter.Filename);
				fixFilenameCase(fullFilename);
				_generatedFiles.Add(fullFilename.ToLowerInvariant());				
			});
		}
						
		private void fixFilenameCase(string targetFileName)
		{
			var existingFileName = Directory.EnumerateFiles(_arguments.OutDir, targetFileName).Single();
			if (targetFileName != Path.GetFileName(existingFileName))
			{
				// filenames differ in case, renaming
				new FileInfo(existingFileName).MoveTo(Path.Combine(_arguments.OutDir, targetFileName));
			}
		}
		
		private void cleanupOutDir()
		{
			var filesToDelete = Directory
				.EnumerateFiles(_arguments.OutDir, "*.*", SearchOption.AllDirectories)
				.Where(file => !_generatedFiles.Contains(file.ToLowerInvariant()))
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
		
		private HashSet<string> _generatedFiles;
	}
}