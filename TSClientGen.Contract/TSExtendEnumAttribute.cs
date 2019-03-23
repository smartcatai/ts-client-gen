using System;
using System.Collections.Generic;
using System.Text;

namespace TSClientGen
{
	/// <summary>
	/// For applying to assembly.
	/// When inherited, allows for appending static members to the generated Typescript enum definition
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public abstract class TSExtendEnumAttribute : Attribute
	{
		protected TSExtendEnumAttribute(Type enumType)
		{
			if (enumType == null) throw new ArgumentNullException(nameof(enumType));
			if (!enumType.IsEnum)
				throw new ArgumentException($"Parameter must be an enum type ({enumType.FullName})", nameof(enumType));

			EnumType = enumType;
		}

		/// <summary>
		/// Enum type
		/// </summary>
		public Type EnumType { get; }

		/// <summary>
		/// Generates TypeScript code (static members for the enum)
		/// </summary>
		public abstract void GenerateStaticMembers(StringBuilder sb);
	}
}
