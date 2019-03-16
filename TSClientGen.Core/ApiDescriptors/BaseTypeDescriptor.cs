using System;

namespace TSClientGen.ApiDescriptors
{
	/// <summary>
	/// Descriptor for generating complex TypeScript types - types or interfaces
	/// </summary>
	public abstract class BaseTypeDescriptor
	{
		protected BaseTypeDescriptor(Type type)
		{
			Type = type;
		}
		
		/// <summary>
		/// .Net тип, который следует преобразовать в TS тип
		/// </summary>
		public Type Type { get; }
	}
}