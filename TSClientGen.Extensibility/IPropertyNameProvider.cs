using System.Reflection;

namespace TSClientGen.Extensibility
{
	/// <summary>
	/// Extensibility point for providing names for properties in generated TypeScript interfaces
	/// </summary>
	public interface IPropertyNameProvider
	{
		/// <summary>
		/// Return property name in TypeScript given an information about a .net type property
		/// </summary>
		/// <param name="propertyInfo">.net property info</param>
		string GetPropertyName(PropertyInfo propertyInfo);
	}
}