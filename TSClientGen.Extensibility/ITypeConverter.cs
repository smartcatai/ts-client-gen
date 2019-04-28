using System;

namespace TSClientGen.Extensibility
{
	/// <summary>
	/// Extensibility point for providing custom mappings of .net types to TypeScript types.
	/// This is for simple case that writes the resulting TypeScript type inline and does not create any type or interface definitions.
	/// </summary>
	public interface ITypeConverter
	{
		/// <summary>
		/// Returns a TypeScript type for a given .net type
		/// </summary>
		/// <param name="type">.net type to get TypeScript type for</param>
		/// <param name="builtInConvert">default builtin type conversion implementation</param>
		string Convert(Type type, Func<Type, string> builtInConvert);
	}
}