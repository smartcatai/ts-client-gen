using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to api method.
	/// Generates an addition TypeScript method for getting the api call url provided with its parameters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class TSGenerateUrlAttribute : Attribute
	{
	}
}