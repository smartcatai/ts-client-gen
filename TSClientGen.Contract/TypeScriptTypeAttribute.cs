using System;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут, которым помечаются классы, для которых нужно поменять логику 
	/// их маппинга на тип в TypeScript. Есть 2 варианта - явно указать строку с TS типом,
	/// либо тип .NET, который будет использован при сериализации.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
	public class TypeScriptTypeAttribute : Attribute
	{
		public TypeScriptTypeAttribute(Type substituteType)
		{
			SubstituteType = substituteType;
		}

		public TypeScriptTypeAttribute(string typeDefinition)
		{
			TypeDefinition = typeDefinition;
		}

		public Type SubstituteType { get; private set; }

		public string TypeDefinition { get; private set; }
	}
}