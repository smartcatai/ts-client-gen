using System;
using System.Collections.Generic;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут для генерации JSON-модуля со статическим контентом
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public abstract class TSStaticContentAttribute : Attribute
	{
		protected TSStaticContentAttribute(string moduleName, Dictionary<string, object> content)
		{
			ModuleName = moduleName;
			Content = content;
		}

		public string ModuleName { get; private set; }

		public Dictionary<string, object> Content { get; private set; }
	}
}
