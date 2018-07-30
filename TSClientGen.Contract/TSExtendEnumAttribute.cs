using System;
using System.Collections.Generic;
using System.Text;

namespace TSClientGen
{
	/// <summary>
	/// Добавляет в генерируемое TypeScript-описание енума статические члены
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

		public Type EnumType { get; }
		
		public IReadOnlyCollection<string> Imports { get; }

		/// <summary>
		/// Генерация TypeScript-кода (статических членов енума)
		/// </summary>
		public abstract void GenerateStaticMembers(StringBuilder sb);
	}
}
