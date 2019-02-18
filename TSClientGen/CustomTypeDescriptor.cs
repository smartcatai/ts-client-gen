using System;
using System.Collections.Generic;

namespace TSClientGen
{
	/// <summary>
	/// Дескриптор для генерации ts-типов
	/// </summary>
	public sealed class CustomTypeDescriptor
	{
		/// <summary>
		/// Конструктор для генерации ts-интерфейса из произвольного .Net типа
		/// </summary>
		public CustomTypeDescriptor(Type type, Type baseType): this(type, null, null, baseType, null)
		{
		}
		
		/// <summary>
		/// Конструктор для генерации базового ts-интерфейса 
		/// (того, на котором висит <see cref="TypeScriptTypeAttribute"/> с <see cref="TypeScriptTypeAttribute.InheritedTypes"/>)
		/// </summary>
		public CustomTypeDescriptor(
			Type type, 
			Type discriminatorFieldType,
			string discriminatorFieldName,
			Type baseType):this(type, discriminatorFieldType, discriminatorFieldName, baseType, null)
		{
		}
		
		/// <summary>
		/// Конструктор для генерации type definition
		/// </summary>
		public CustomTypeDescriptor(
			Type type, 
			string typeDefinition): this(type, null, null, null, typeDefinition)
		{
		}

		private CustomTypeDescriptor(
			Type type,
			Type discriminatorFieldType,
			string discriminatorFieldName,
			Type baseType,
			string typeDefinition)
		{
			Type = type;
			DiscriminatorFieldType = discriminatorFieldType;
			DiscriminatorFieldName = discriminatorFieldName;
			BaseType = baseType;
			TypeDefinition = typeDefinition;
		}

		/// <summary>
		/// .Net тип, который следует преобразовать в TS тип
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Тип свойства-дискиминатора
		/// </summary>
		public Type DiscriminatorFieldType { get; }
		
		/// <summary>
		/// Название свойства-дискриминатора 
		/// </summary>
		public string DiscriminatorFieldName { get; }

		/// <summary>
		/// Базовый класс для генерируемого <see cref="Type"/>
		/// </summary>
		public Type BaseType { get; }
		
		/// <summary>
		/// Сторка для генерации type definition 
		/// </summary>
		public string TypeDefinition { get; }
	}
}
