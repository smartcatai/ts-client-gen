using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to api method, its parameter or to a property of a model class.
	/// Excludes api method, api method parameter or the property of a type from TypeScript code generation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property)]
	public class TSIgnoreAttribute : Attribute
	{
	}
}