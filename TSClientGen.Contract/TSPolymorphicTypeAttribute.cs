using System;
using System.Reflection;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут, которым помечаются класс, иерархия наследников которого должна быть сохранена в TS 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class TSPolymorphicTypeAttribute : Attribute
	{
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="discriminatorFieldType">Enum, который будет типом свойства, идентифицирующего конкретный inheritedType</param>
		/// <param name="descendantsAssemblyType">
		/// Тип, находящийся в сборке, откуда надо барть наследников (по умолчанию берутся из сборки, где находится тип, помеченный данным атрибутом
		/// </param>
		/// <param name="discriminatorFieldName">название свойства в TS типе, где будет храниться информация, идентифицирующая конкретный inheritedType</param>
		/// <param name="suppressDiscriminatorGeneration">Не генерировать дискриминатор - предполагается что класс уже содержит его</param>
		public TSPolymorphicTypeAttribute(
			Type discriminatorFieldType = null, 
			Type descendantsAssemblyType = null, 
			string discriminatorFieldName = "type",
			bool suppressDiscriminatorGeneration = false)
		{
			if (!suppressDiscriminatorGeneration)
			{
				if (discriminatorFieldType == null)
					throw new ArgumentNullException(nameof(discriminatorFieldType));
			
				if (!discriminatorFieldType.IsEnum)
					throw new ArgumentException($"{nameof(discriminatorFieldType)} must by Enum type.");
			
				if (string.IsNullOrWhiteSpace(discriminatorFieldName))
					throw new ArgumentException($"{nameof(discriminatorFieldName)} is null or whitespace");
			}
			
			DescendantsAssembly = descendantsAssemblyType?.Assembly;
			DiscriminatorFieldType = discriminatorFieldType;
			DiscriminatorFieldName = discriminatorFieldName;
			SuppressDiscriminatorGeneration = suppressDiscriminatorGeneration;
		}
		
		public Assembly DescendantsAssembly { get; }

		public Type DiscriminatorFieldType { get; }
		
		public string DiscriminatorFieldName { get; }
		
		public bool SuppressDiscriminatorGeneration { get; }
	}
}
