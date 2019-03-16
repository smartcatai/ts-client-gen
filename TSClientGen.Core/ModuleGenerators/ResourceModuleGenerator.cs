using System;
using System.Collections.Generic;

namespace TSClientGen
{
	public class ResourceModuleGenerator
	{
		public ResourceModuleGenerator(IResourceModuleWriter writer)
		{
			_writer = writer;
		}
		
		public void WriteEnumLocalizations(IReadOnlyCollection<TSEnumLocalizationAttribute> enumLocalizations)
		{
			foreach (var enumLocalization in enumLocalizations)
			{
				generateEnumResxEntries(enumLocalization);
				if (enumLocalization.AdditionalContexts != null)
				{
					foreach (var context in enumLocalization.AdditionalContexts)
					{
						generateEnumResxEntries(enumLocalization, context);
					}
				}
			}
		}
		
		private void generateEnumResxEntries(TSEnumLocalizationAttribute enumLocalization, string context = null)
		{
			var enumName = enumLocalization.EnumType.Name;
			foreach (var valueName in Enum.GetNames(enumLocalization.EnumType))
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