using System;

namespace TSClientGen
{
	/// <summary>
	/// Аттрибут для разрешения неоднозначности enum'ов - так мы явно указываем, что
	/// enum относится к модулю проекта
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class TypeScriptEnumModuleAttribute : Attribute
	{
		public TypeScriptEnumModuleAttribute(Type enumType)
		{
			EnumType = enumType;
		}

		public Type EnumType { get; }
	}
}