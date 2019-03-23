using System;

namespace TSClientGen.ApiDescriptors
{
	/// <summary>
	/// Descriptor for writing handwritten TypeScript type definitions provided with a <see cref="TSSubstituteTypeAttribute"/>
	/// </summary>
	public sealed class HandwrittenTypeDescriptor : BaseTypeDescriptor
	{		
		/// <summary>
		/// ctor
		/// </summary>
		public HandwrittenTypeDescriptor(Type type, string typeDefinition): base(type)
		{
			TypeDefinition = typeDefinition;
		}

		
		/// <summary>
		/// Handwritten TypeScript type definition for a .net type 
		/// </summary>
		public string TypeDefinition { get; }
	}
}
