using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSClientGen
{
	public class EnumModuleGenerator
	{
		public void Write(
			IEnumerable<Type> enumTypes,
			bool useStringEnums,
			string getResourceModuleName,
			Dictionary<Type, List<Func<string>>> staticMemberGeneratorsByEnumType,
			Dictionary<Type, TSEnumLocalizationAttributeBase> localizationByEnumType)
		{
			var requireResourceImport = false;
			var typesWithStaticMembers = new List<Type>();

			foreach (var enumType in enumTypes)
			{
				writeEnum(enumType, useStringEnums);

				if (staticMemberGeneratorsByEnumType.ContainsKey(enumType) || localizationByEnumType.ContainsKey(enumType))
				{
					typesWithStaticMembers.Add(enumType);
				}
			}

			foreach (var enumType in typesWithStaticMembers)
			{
				_result.AppendLine($"export namespace {enumType.Name} {{").Indent();

				if (localizationByEnumType.TryGetValue(enumType, out var localizationProvider))
				{
					writeEnumLocalizationFunctions(enumType, localizationProvider.AdditionalContexts, localizationProvider.UsePrefix, _result);
					requireResourceImport = true;
				}

				if (staticMemberGeneratorsByEnumType.TryGetValue(enumType, out var staticMembersGenerators))
				{
					foreach (var generateStaticMembers in staticMembersGenerators)
					{
						_result.AppendText(generateStaticMembers());
					}
				}

				_result.Unindent().AppendLine("}");
			}

			if (requireResourceImport)
			{
				if (string.IsNullOrEmpty(getResourceModuleName))
					throw new InvalidOperationException(
						"You should provide get-resource-module command line parameter if you have TSEnumLocalizationAttribute in your codebase");

				_result.AppendLine($"import {{ getResource }} from '{getResourceModuleName}';");
			}
		}

		private void writeEnumLocalizationFunctions(
			Type enumType, string[] additionalContexts, bool usePrefix, IndentedStringBuilder sb)
		{
			if (additionalContexts == null)
			{
				sb
					.AppendLine($"export function localize(enumValue: {enumType.Name}) {{")
					.Indent()
					.AppendLine($"return getResource('{enumType.Name}_' + {enumType.Name}[enumValue]);")
					.Unindent()
					.AppendLine("}");
			}
			else
			{
				var contextTypeScriptType = string.Join("|", additionalContexts.Select(s => $"'{s}'"));
				sb
					.AppendLine($"export function localize(enumValue: {enumType.Name}, context?: {contextTypeScriptType}) {{")
					.Indent()
					.AppendLine($"const prefix = '{enumType.Name}_' + (context ? context + '_' : '');")
					.AppendLine($"return getResource(prefix + {enumType.Name}[enumValue]);")
					.Unindent()
					.AppendLine("}");
			}

			sb
				.AppendLine("export function getLocalizedValues() {")
				.Indent()
				.AppendLine(
					$"return Object.keys({enumType.Name}).map(key => parseInt(key)).filter(key => !isNaN(key)).map(id => {{")
				.Indent()
				.AppendLine("return {")
				.Indent()
				.AppendLine("id: id,")
				.AppendLine($"name: {enumType.Name}.localize(id)")
				.Unindent()
				.AppendLine("};")
				.Unindent()
				.AppendLine("});")
				.AppendLine("}");
		}

		private void writeEnum(Type enumType, bool useStringEnums)
		{
			var names = Enum.GetNames(enumType);
			var underlyingType = Enum.GetUnderlyingType(enumType);

			_result.AppendLine($"export enum {enumType.Name} {{").Indent();

			foreach (string name in names)
			{
				var value = useStringEnums
					? $"'{name}'"
					: Convert.ChangeType(Enum.Parse(enumType, name), underlyingType);
				_result.AppendLine($"{name} = {value},");
			}

			_result.Unindent().AppendLine("}").AppendLine();
		}

		public string GetResult()
		{
			return _result.ToString();
		}


		private readonly IndentedStringBuilder _result = new IndentedStringBuilder();
	}
}