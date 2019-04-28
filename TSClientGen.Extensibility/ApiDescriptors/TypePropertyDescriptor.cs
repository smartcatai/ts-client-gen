using System;
using System.Reflection;

namespace TSClientGen.Extensibility.ApiDescriptors
{
	/// <summary>
	/// Describes TypeScript interface property
	/// </summary>
	public class TypePropertyDescriptor
	{
		public TypePropertyDescriptor(string name, Type type, string inlineTypeDefinition = null)
		{
			Name = name;
			Type = type;
			InlineTypeDefinition = inlineTypeDefinition;
		}

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