using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to model type.
	/// Forces specified type's descendants found in the same or explicitly specified assembly
	/// to be appended the api client module.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class TSRequireDescendantTypes: Attribute
	{
		/// <summary>
		/// Scan the specified type's assembly for the descendants of required type
		/// and append them to the api client module as well 
		/// </summary>
		public Type IncludeDescendantsFromAssembly { get; }
	}
}