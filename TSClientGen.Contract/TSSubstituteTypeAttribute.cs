using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to model type or property.
	/// Specifies that this model type or property should be handled as a different type when generating a TypeScript definition for a model.
	/// You can specify either another .net Type or a string with a handwritten TypeScript type definition.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
	public class TSSubstituteTypeAttribute : Attribute
	{
		/// <summary>
		/// Specifies another .net type to substitute original type with
		/// </summary>
		public TSSubstituteTypeAttribute(Type substituteType)
		{
			if (substituteType == null) throw new ArgumentNullException(nameof(substituteType));
			
			SubstituteType = substituteType;
		}

		/// <summary>
		/// Specifies handwritten TypeScript type declaration to substitute original type with
		/// </summary>
		public TSSubstituteTypeAttribute(string typeDefinition, bool inline = false)
		{
			if (string.IsNullOrWhiteSpace(typeDefinition)) throw new ArgumentNullException(nameof(typeDefinition));
			
			TypeDefinition = typeDefinition;
			Inline = inline;
		}	
	
		/// <summary>
		/// Another .net type to substitute original type with
		/// </summary>
		public Type SubstituteType { get; private set; }
		
		/// <summary>
		/// Handwritten TypeScript type declaration to substitute original type with
		/// </summary>
		public string TypeDefinition { get; private set; }

		/// <summary>
		/// Whether the type definition should be inlined at each usage of the type
		/// or be contained in a separate type declaration 
		/// </summary>
		public bool Inline { get; }
	}
}
