using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSClientGen
{
	public class EnumModuleGenerator
	{
		public void Write(
			Type enumType,
			bool useStringEnums,
			string getResourceModuleName,
			StaticMembers staticMembers,
			TSEnumLocalizationAttributeBase localization)
		{
			if (staticMembers?.EnumImportTypes?.Any() == true)
			{
				foreach (var importEnumType in staticMembers.EnumImportTypes)
					_result.AppendLine($"import {{ {importEnumType.Name} }} from '../{importEnumType.Name}';");

				_result.AppendLine();
			}

			writeEnum(enumType, useStringEnums);

			var requireResourceImport = false;

			if (staticMembers?.Generators?.Any() == true || localization != null)
			{
				_result.AppendLine($"export namespace {enumType.Name} {{").Indent();

				if (localization != null)
				{
					writeEnumLocalizationFunctions(enumType, localization.AdditionalSets, _result);
					requireResourceImport = true;
				}

				if (staticMembers?.Generators?.Any() == true)
				{
					foreach (var generateStaticMembers in staticMembers.Generators)
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

		private void writeEnumLocalizationFunctions(Type enumType, string[] additionalSets, IndentedStringBuilder sb)
		{
			if (additionalSets == null)
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
				var setTsType = string.Join("|", additionalSets.Select(s => $"'{s}'"));
				sb
					.AppendLine($"export function localize(enumValue: {enumType.Name}, set?: {setTsType}) {{")
					.Indent()
					.AppendLine($"const prefix = '{enumType.Name}_' + (set ? set + '_' : '');")
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
				.Unindent()
				.AppendLine("}")
				;
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