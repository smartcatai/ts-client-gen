using System;

namespace TSClientGen.ApiDescriptors
{
	/// <summary>
	/// Descriptor for generating complex TypeScript types
	/// </summary>
	public sealed class CustomTypeDescriptor : BaseTypeDescriptor
	{		
		/// <summary>
		/// Конструктор для генерации type definition
		/// </summary>
		public CustomTypeDescriptor(Type type, string typeDefinition): base(type)
		{
			TypeDefinition = typeDefinition;
		}

		
		/// <summary>
		/// Сторка для генерации type definition 
		/// </summary>
		public string TypeDefinition { get; }
	}
}
