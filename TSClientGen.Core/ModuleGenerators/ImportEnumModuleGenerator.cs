using System;
using System.Collections.Generic;

namespace TSClientGen
{
	public class ImportEnumModuleGenerator : IImportEnumModuleGenerator
	{
		private readonly IndentedStringBuilder _result = new IndentedStringBuilder();

		public string Generate(IReadOnlyCollection<Type> enumTypes, string enumModuleName)
		{
			foreach (var enumType in enumTypes)
			{
				var enumPath = $"{enumModuleName}/{enumType.Name}";
				_result.AppendLine($"import {{ {enumType.Name} }} from './{enumPath}'");
			}

			_result.AppendLine("export {");

			foreach (var enumType in enumTypes)
				_result.AppendLine($"\t{enumType.Name},");

			_result.AppendLine("}");

			return _result.ToString();
		}
	}
}