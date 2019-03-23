using System;
using System.Collections.Generic;

namespace TSClientGen
{
	/// <summary>
	/// For applying to assembly.
	/// Generates a separate JSON module with named exports of an arbitrary content 
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public abstract class TSStaticContentAttribute : Attribute
	{
		protected TSStaticContentAttribute(string moduleName, Dictionary<string, object> content)
		{
			ModuleName = moduleName;
			Content = content;
		}

		/// <summary>
		/// Name of the module to generate
		/// </summary>
		public string ModuleName { get; }

		/// <summary>
		/// Content to write to the module. Key - named export key, value - JSON-serializable content
		/// </summary>
		public Dictionary<string, object> Content { get; }
	}
}
