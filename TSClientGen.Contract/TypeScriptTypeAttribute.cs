using System;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут, которым помечаются классы, для которых нужно поменять логику 
	/// их маппинга на тип в TypeScript.
	/// </summary>
	/// <remarks>
	/// - можно явно указать строку с TS типом
	/// - можно указать любой тип .Net, который будет использоваться при сериализации
	/// - можно указать множество типов .Net, которые будут использованы для составления union type.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
	public class TypeScriptTypeAttribute : Attribute
	{
		public TypeScriptTypeAttribute(Type substituteType)
		{
			SubstituteType = substituteType;
			InheritedTypes = new Type[0];
		}

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="discriminatorFieldType">Enum, который будет типом свойства, идентифицирующего конкретный inheritedType</param>
		/// <param name="inheritedTypes">массив производных замещающих типов</param>
		/// <param name="discriminatorFieldName">название свойства в TS типе, где будет храниться информация, идентифицирующая конкретный inheritedType</param>
		public TypeScriptTypeAttribute(Type discriminatorFieldType, Type[] inheritedTypes, string discriminatorFieldName = "type")
		{
			if (discriminatorFieldType == null)
			{
				throw new ArgumentNullException(nameof(discriminatorFieldType));
			}

			if (!discriminatorFieldType.IsEnum)
			{
				throw new ArgumentException($"{nameof(TypeDefinition)} must by Enum type.");
			}
			
			if (string.IsNullOrWhiteSpace(discriminatorFieldName))
			{
				throw new ArgumentException($"{nameof(discriminatorFieldName)} is null or whitespace");
			}
			
			InheritedTypes = inheritedTypes;
			DiscriminatorFieldType = discriminatorFieldType;
			DiscriminatorFieldName = discriminatorFieldName;
		}
		
		public TypeScriptTypeAttribute(string typeDefinition)
		{
			TypeDefinition = typeDefinition;
			InheritedTypes = new Type[0];
		}	
		
		public Type SubstituteType { get; private set; }
		
		public Type[] InheritedTypes { get; private set; }

		public string TypeDefinition { get; private set; }
		
		public Type DiscriminatorFieldType { get; private set; }
		
		public string DiscriminatorFieldName { get; private set; }
	}
}
