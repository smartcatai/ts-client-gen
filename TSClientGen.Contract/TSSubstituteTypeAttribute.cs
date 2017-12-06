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
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
	public class TSSubstituteTypeAttribute : Attribute
	{
		public TSSubstituteTypeAttribute(Type substituteType)
		{
			if (substituteType == null) throw new ArgumentNullException(nameof(substituteType));
			
			SubstituteType = substituteType;
		}

		public TSSubstituteTypeAttribute(string typeDefinition)
		{
			if (string.IsNullOrWhiteSpace(typeDefinition)) throw new ArgumentNullException(nameof(typeDefinition));
			
			TypeDefinition = typeDefinition;
		}	
		
		public Type SubstituteType { get; private set; }
		
		public string TypeDefinition { get; private set; }
	}
}
