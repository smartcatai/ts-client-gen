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
		protected TSExtendEnumAttribute(Type enumType, IReadOnlyCollection<string> imports = null)
		{
			if (enumType == null) throw new ArgumentNullException(nameof(enumType));
			if (!enumType.IsEnum)
				throw new ArgumentException($"Parameter must be an enum type ({enumType.FullName})", nameof(enumType));

			EnumType = enumType;
			Imports = imports ?? new string[0];
		}

		public Type EnumType { get; }
		
		public IReadOnlyCollection<string> Imports { get; }

		/// <summary>
		/// Генерация TypeScript-кода (статических членов енума)
		/// </summary>
		public abstract void GenerateStaticMembers(StringBuilder sb);
	}
}
