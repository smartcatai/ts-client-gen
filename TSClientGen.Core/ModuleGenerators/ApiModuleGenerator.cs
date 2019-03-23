using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TSClientGen.ApiDescriptors;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class ApiModuleGenerator
	{
		public ApiModuleGenerator(
			ModuleDescriptor module,
			TypeMapping typeMapping,
			IPropertyNameProvider propertyNameProvider,
			Func<object, string> serializeToJson,
			string commonModuleName)
		{
			_module = module;
			_typeMapping = typeMapping;
			_propertyNameProvider = propertyNameProvider;
			_serializeToJson = serializeToJson;
			_commonModuleName = commonModuleName;
		}
		
		
		public void WriteApiClientClass()
		{
			var imports = new List<string> { "request" };
			if (_module.Methods.Any(m => !m.UploadsFiles))
			{
				imports.Add("HttpRequestOptions");				
			}
			if (_module.Methods.Any(m => m.UploadsFiles))
			{
				imports.Add("UploadFileHttpRequestOptions");
				imports.Add("NamedBlob");
			}
			if (_module.Methods.Any(m => m.GenerateUrl))
			{
				imports.Add("getUri");
			}
			
			_result
				.AppendLine($"import {{ {string.Join(", ", imports)} }} from '{_commonModuleName}';")
				.AppendLine()
				.AppendLine($"export class {_module.ApiClientClassName} {{")
				.Indent();
			
			if (_module.SupportsExternalHost)
			{
				_result.AppendLine("constructor(private hostname?: string) {}").AppendLine();
			}
			
			foreach (var method in _module.Methods)
			{
				var methodWriter = new ApiMethodGenerator(method, _result, _typeMapping);
				methodWriter.ResolveConflictingParamNames(imports);
				if (method.GenerateUrl)
				{					
					writeMethod(
						() => methodWriter.WriteGetUrlSignature(), 
						() => methodWriter.WriteBody(true, _module.SupportsExternalHost));
				}

				writeMethod(
					() => methodWriter.WriteSignature(),
					() => methodWriter.WriteBody(false, _module.SupportsExternalHost));
			}

			_result
				.Unindent()
				.AppendLine("}").AppendLine()
				.AppendLine($"export default new {_module.ApiClientClassName}();").AppendLine();
		}

		public void WriteTypeDefinitions()
		{
			var typesToGenerate = new HashSet<Type>();
			var generatedTypes = new HashSet<Type>();

			// generating typescript interface recursively processes .net interface's nested properties
			// and new types can be added to type mapping during this processing
			// that's why we iterate over all types in type mapping in a loop until exhausting them all
			do
			{
				var knownTypes = _typeMapping.GetTypesToGenerate().Where(t => !t.IsEnum);
				typesToGenerate.UnionWith(knownTypes);
				typesToGenerate.ExceptWith(generatedTypes);
					
				foreach (var type in typesToGenerate)
				{
					var typeDescriptor = _typeMapping.GetDescriptorByType(type);
					switch (typeDescriptor)
					{
						case HandwrittenTypeDescriptor typeDesc:
							WriteType(typeDesc);
							break;
						case InterfaceDescriptor interfaceDesc:
							WriteInterface(interfaceDesc);
							break;
						default:
							throw new Exception("Unexpected type descriptor - " +
							                    typeDescriptor.GetType().FullName);
					}
					generatedTypes.Add(type);
				}
			} while (typesToGenerate.Any());
		}

		public void WriteType(HandwrittenTypeDescriptor handwrittenType)
		{
			_result.AppendLine($"export type {_typeMapping.GetTSType(handwrittenType.Type)} = {handwrittenType.TypeDefinition}").AppendLine();
		}
		
		public void WriteInterface(InterfaceDescriptor type)
		{
			_result.Append($"export interface {_typeMapping.GetTSType(type.Type)} ");

			if (type.BaseType != null)
			{
				_result.Append($"extends {_typeMapping.GetTSType(type.Type.BaseType)} ");
			}
			
			_result.AppendLine("{").Indent();

			if (!string.IsNullOrWhiteSpace(type.DiscriminatorFieldName))
			{
				var discriminatorType = _typeMapping.GetTSType(type.DiscriminatorFieldType);
				_result.AppendLine($"{type.DiscriminatorFieldName}: {discriminatorType};");
			}
			
			foreach (var property in type.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				var name = _propertyNameProvider.GetPropertyName(property);
				if (name == null)
					continue;

				if (name == type.DiscriminatorFieldName)
				{
					throw new InvalidOperationException(
						$"Discriminator field name `{type.DiscriminatorFieldName}` can be equal property name `{type.Type.Name}::{name}`");
				}

				if (Nullable.GetUnderlyingType(property.PropertyType) != null)
					name += "?";

				_result.AppendLine($"{name}: {_typeMapping.GetTSType(property)};");
			}

			_result.Unindent().AppendLine("}").AppendLine();
		}

		public void WriteEnumImports(string enumsModuleName)
		{
			var enumTypes = _typeMapping.GetTypesToGenerate().Where(t => t.IsEnum).ToList();
			if (enumTypes.Any())
			{
				_result.Append("import { ")
					.Append(string.Join(", ", enumTypes.Select(t => t.Name)))
					.AppendLine($" }} from './{enumsModuleName}'")
					.AppendLine();
			}
		}
		
		public void WriteStaticContent(TSStaticContentAttribute staticContentModule)
		{
			foreach (var entry in staticContentModule.Content)
			{
				_result.AppendLine($"export let {entry.Key} = {_serializeToJson(entry.Value)};");
			}
		}
		
		public string GetResult()
		{
			return _result.ToString();
		}
		
		
		private void writeMethod(Action writeSignature, Action writeBody)
		{
			writeSignature();
			_result.AppendLine(" {").Indent();
			writeBody();
			_result.Unindent().AppendLine("}").AppendLine();
		}		
		
		private readonly ModuleDescriptor _module;
		private readonly TypeMapping _typeMapping;
		private readonly IPropertyNameProvider _propertyNameProvider;
		private readonly Func<object, string> _serializeToJson;
		private readonly string _commonModuleName;
		private readonly IndentedStringBuilder _result = new IndentedStringBuilder();
	}
}