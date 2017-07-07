using System;
using System.Collections.Generic;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут для генерации JSON-модуля со статическим контентом
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public abstract class TypeScriptStaticContentAttribute : Attribute
	{
		protected TypeScriptStaticContentAttribute(string moduleName, Dictionary<string, object> content)
		{
			ModuleName = moduleName;
			Content = content;
		}

		public string ModuleName { get; private set; }

		public Dictionary<string, object> Content { get; private set; }
	}
}
