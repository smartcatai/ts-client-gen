using System;

namespace TSClientGen
{
	/// <summary>
	/// Аттрибут для указания, каким классам мы хотим сгенерить ts-модели
	/// </summary>
	/// <remarks>
	/// Используется, когда нужно сгенерить модели для классов, 
	/// которые не используются в качестве типов аргументов контроллеров.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class TypeScriptGenerateClassModelAttribute : Attribute
	{
		public TypeScriptGenerateClassModelAttribute(Type enumType)
		{
			EnumType = enumType;
		}

		public Type EnumType { get; }
	}
}
