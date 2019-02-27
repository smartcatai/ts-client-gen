using System;
using System.Resources;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут, с помощью которого можно указать ресурсный файл, который должен быть доступен клиенту
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class TSExposeResxAttribute : Attribute
	{
		public TSExposeResxAttribute(Type resxType)
		{
			ResxName = resxType.Name;
			ResourceManager = resxType.GetResourceManager();
		}

		public ResourceManager ResourceManager { get; }

		public string ResxName { get; }
	}
}