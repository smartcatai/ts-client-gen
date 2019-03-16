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
			ILookup<Type, TSExtendEnumAttribute> staticMemberProvidersByEnum,
			TypeMapping mapping,
			string defaultLocale)
		{
			var requireResourceImport = false;

			foreach (var @enum in enumTypes)
			{
				writeEnum(@enum, mapping);

				if (!staticMemberProvidersByEnum[@enum].Any())
					continue;

				_result.AppendLine($"export namespace {@enum.Name} {{");

				foreach (var provider in staticMemberProvidersByEnum[@enum])
				{
					provider.GenerateStaticMembers(_result);

					if (provider is TSEnumLocalizationAttribute)
					{
						requireResourceImport = true;
					}
				}

				_result.AppendLine("}");
			}

			if (requireResourceImport)
			{
				_result.AppendLine();
				_result.AppendLine("function getResource(key: string) {");
				_result.AppendLine("\tlet locale = (<any>window).locale;");
				_result.AppendLine($"\tlet value = resx.messages[locale][key] || resx.messages['{defaultLocale}'][key];");
				_result.AppendLine("\tif (!value) console.warn('Key ' + key + ' has not been found in enums.resx');");
				_result.AppendLine("\treturn value || key;");
				_result.AppendLine("}");
				_result.AppendLine("import resx from '../enums.resx'");
			}
		}

		private void writeEnum(Type enumType, TypeMapping mapping)
		{
			var names = Enum.GetNames(enumType);
			var underlyingType = Enum.GetUnderlyingType(enumType);

			_result.AppendLine($"export enum {mapping.GetTSType(enumType)} {{");

			foreach (string name in names)
			{
				var value = Convert.ChangeType(Enum.Parse(enumType, name), underlyingType);
				_result.AppendLine($"\t{name} = {value},");
			}

			_result.AppendLine("}").AppendLine();
		}

		public string GetResult()
		{
			return _result.ToString();
		}

		
		private readonly StringBuilder _result = new StringBuilder();
	}
}