using System;
using System.Reflection;

namespace TSClientGen.Extensibility.ApiDescriptors
{
	/// <summary>
	/// Describes TypeScript interface property
	/// </summary>
	public class TypePropertyDescriptor
	{
		public TypePropertyDescriptor(string name, Type type, bool isNullable, bool isOptional, string inlineTypeDefinition = null)
		{
			Name = name;
			Type = type;
			IsNullable = isNullable;
			IsOptional = isOptional;
			InlineTypeDefinition = inlineTypeDefinition;
		}

		/// <summary>
		/// Does property accept `null` as a correct value
		/// </summary>
		public bool IsNullable { get; }

		/// <summary>
		/// Could property be omitted from JSON
		/// </summary>
		public bool IsOptional { get; }
		
		/// <summary>
		/// Property name
		/// </summary>
		public string Name { get; }
				
		/// <summary>
		/// .Net type of a property
		/// </summary>
		public Type Type { get; }
		
		/// <summary>
		/// Inline TypeScript property type definition
		/// </summary>
		public string InlineTypeDefinition { get; }
	}
}