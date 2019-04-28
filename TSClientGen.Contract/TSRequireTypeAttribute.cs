using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to api controller.
	/// Forces appending specified type definition to the api client module.
	/// </summary>
	/// <remarks>
	/// Can be used to force writing enums to Typescript even if they aren't used by any api methods.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class TSRequireTypeAttribute : Attribute
	{
		public TSRequireTypeAttribute(Type type)
		{
			GeneratedType = type;
		}
		
		public Type GeneratedType { get; }

		/// <summary>
		/// Scan the specified type's assembly for the descendants of required type
		/// and append them to the api client module as well 
		/// </summary>
		public Type IncludeDescendantsFromAssembly { get; set; }
	}
}
