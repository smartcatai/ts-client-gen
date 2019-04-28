using System;
using System.Collections.Generic;

namespace TSClientGen.Extensibility.ApiDescriptors
{
	/// <summary>
	/// Describes the TypeScript interface for a .net type 
	/// </summary>
	public class TypeDescriptor
	{
		public TypeDescriptor(Type type, IReadOnlyCollection<TypePropertyDescriptor> properties)
		{
			BaseType = type.BaseType != typeof(object)
			           && type.BaseType != typeof(ValueType)
			           && type.BaseType != typeof(Enum)
				? type.BaseType
				: null;
			Properties = properties;
		}
		
		/// <summary>
		/// Base class for this .net type
		/// </summary>
		public Type BaseType { get; }

		/// <summary>
		/// Properties of the interface
		/// </summary>
		public IReadOnlyCollection<TypePropertyDescriptor> Properties { get; }
	}
}