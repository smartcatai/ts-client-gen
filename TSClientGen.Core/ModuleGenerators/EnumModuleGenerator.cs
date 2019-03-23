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
			string getResourceModuleName,
			ILookup<Type, TSExtendEnumAttribute> staticMemberProvidersByEnum)
		{
			var requireResourceImport = false;

			foreach (var @enum in enumTypes)
			{
				writeEnum(@enum);

				if (!staticMemberProvidersByEnum[@enum].Any())
					continue;

				_result.AppendLine($"export namespace {@enum.Name} {{").Indent();

				foreach (var provider in staticMemberProvidersByEnum[@enum])
				{
					var staticMembersContent = new StringBuilder();
					provider.GenerateStaticMembers(staticMembersContent);
					_result.AppendText(staticMembersContent.ToString());

					if (provider is TSEnumLocalizationAttribute)
					{
						requireResourceImport = true;
					}
				}

				_result.Unindent().AppendLine("}");
			}

			if (requireResourceImport)
			{
				_result.AppendLine($"import {{ getResource }} from '{getResourceModuleName}';");
			}
		}

		private void writeEnum(Type enumType)
		{
			var names = Enum.GetNames(enumType);
			var underlyingType = Enum.GetUnderlyingType(enumType);

			_result.AppendLine($"export enum {enumType.Name} {{").Indent();

			foreach (string name in names)
			{
				var value = Convert.ChangeType(Enum.Parse(enumType, name), underlyingType);
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