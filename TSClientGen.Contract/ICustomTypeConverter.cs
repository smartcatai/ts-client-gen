using System;
using System.Reflection;

namespace TSClientGen
{
	/// <summary>
	/// Extensibility point for providing custom mappings of .net types to TypeScript types
	/// </summary>
	public interface ICustomTypeConverter
	{
		/// <summary>
		/// Returns a TypeScript type for a given .net type
		/// </summary>
		/// <param name="type">.net type to get TypeScript type for</param>
		/// <param name="defaultConvert">default builtin type conversion implementation</param>
		string Convert(Type type, Func<Type, string> defaultConvert);
	}
}