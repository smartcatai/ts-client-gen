using System;
using System.Collections.Generic;
using System.Linq;
using TSClientGen.Extensibility;

namespace TSClientGen
{
	public class EnumResourceModuleGenerator
	{
		public EnumResourceModuleGenerator(IResourceModuleWriter writer)
		{
			_writer = writer;
		}
		
		public void WriteEnumLocalizations(Type enumType, TSEnumLocalizationAttributeBase localization)
		{
			generateEnumValueEntries(enumType, localization);
			if (localization.AdditionalSets != null)
			{
				foreach (var set in localization.AdditionalSets)
				{
					generateEnumValueEntries(enumType, localization, set);
				}
			}
		}
		
		private void generateEnumValueEntries(Type enumType, TSEnumLocalizationAttributeBase enumLocalization, string set = null)
		{
			var enumName = enumType.Name;
			foreach (var valueName in Enum.GetNames(enumType))
			{
				string valueNameWithContext = (set != null) ? $"{set}_{valueName}" : valueName;
				string resourceKey = enumLocalization.UsePrefix ? $"{enumName}_{valueNameWithContext}" : valueNameWithContext;
				var localization = enumLocalization.ResourceManager.GetString(resourceKey);
				if (localization == null && set != null)
				{
					resourceKey = enumLocalization.UsePrefix ? $"{enumName}_{valueName}" : valueName;
					localization = enumLocalization.ResourceManager.GetString(resourceKey);
				}
				if (localization == null)
				{
					throw new Exception(
						$"Enum value {enumName}.{valueName} is not localized in RESX {enumLocalization.ResxName} (set - {set ?? "none"}, key - {resourceKey})");
				}

				_writer.AddResource($"{enumName}_{valueNameWithContext}", localization);
			}
		}
		
		private readonly IResourceModuleWriter _writer;		
	}
}