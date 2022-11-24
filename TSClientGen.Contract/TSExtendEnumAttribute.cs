﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TSClientGen
{
	/// <summary>
	/// For applying to enum type.
	/// When inherited, allows for appending static members to the generated Typescript enum definition
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
	public abstract class TSExtendEnumAttribute : Attribute
	{
		/// <summary>
		/// Enum types to be added in import statements
		/// </summary>
		public IReadOnlyCollection<Type> ImportEnumTypes { get; }

		/// <summary>
		/// Generates TypeScript code (static members for the enum)
		/// </summary>
		public abstract string GenerateStaticMembers();
	}

	namespace ForAssembly
	{
		/// <summary>
		/// For applying to assembly.
		/// When inherited, allows for appending static members to the generated Typescript enum definition
		/// </summary>
		[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
		public abstract class TSExtendEnumAttribute : Attribute
		{
			protected TSExtendEnumAttribute(Type enumType, IReadOnlyCollection<Type> importEnumTypes = null)
			{
				if (enumType == null) throw new ArgumentNullException(nameof(enumType));
				if (!enumType.IsEnum)
					throw new ArgumentException($"Parameter must be an enum type ({enumType.FullName})", nameof(enumType));

				EnumType = enumType;
				ImportEnumTypes = importEnumTypes;
			}

			/// <summary>
			/// Enum type
			/// </summary>
			public Type EnumType { get; }

			/// <summary>
			/// Enum types to be added in import statements
			/// </summary>
			public IReadOnlyCollection<Type> ImportEnumTypes { get; }

			/// <summary>
			/// Generates TypeScript code (static members for the enum)
			/// </summary>
			public abstract string GenerateStaticMembers();
		}
	}
}
