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
		
		public void WriteEnumLocalizations(IDictionary<Type, TSEnumLocalizationAttributeBase> enumLocalizationProvidersByEnumType)
		{
			foreach (var pair in enumLocalizationProvidersByEnumType)
			{
				generateEnumValueEntries(pair.Key, pair.Value);
				if (pair.Value.AdditionalContexts != null)
				{
					foreach (var context in pair.Value.AdditionalContexts)
					{
						generateEnumValueEntries(pair.Key, pair.Value, context);
					}
				}
			}
		}
		
		private void generateEnumValueEntries(Type enumType, TSEnumLocalizationAttributeBase enumLocalization, string context = null)
		{
			var enumName = enumType.Name;
			foreach (var valueName in Enum.GetNames(enumType))
			{
				string valueNameWithContext = (context != null) ? $"{context}_{valueName}" : valueName;
				string resourceKey = enumLocalization.UsePrefix ? $"{enumName}_{valueNameWithContext}" : valueNameWithContext;
				var localization = enumLocalization.ResourceManager.GetString(resourceKey);
				if (localization == null && context != null)
				{
					resourceKey = enumLocalization.UsePrefix ? $"{enumName}_{valueName}" : valueName;
					localization = enumLocalization.ResourceManager.GetString(resourceKey);
				}
				if (localization == null)
				{
					throw new Exception(
						$"Enum value {enumName}.{valueName} is not localized in RESX {enumLocalization.ResxName} (context - {context ?? "none"}, key - {resourceKey})");
				}

				_writer.AddResource($"{enumName}_{valueNameWithContext}", localization);
			}
		}
		
		private readonly IResourceModuleWriter _writer;		
	}
}