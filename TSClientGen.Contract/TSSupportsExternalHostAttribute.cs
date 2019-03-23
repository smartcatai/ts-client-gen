using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to api controller.
	/// Allows for issuing api calls to another domain by providing it as an api client constructor parameter
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TSSupportsExternalHostAttribute : Attribute
	{
	}
}