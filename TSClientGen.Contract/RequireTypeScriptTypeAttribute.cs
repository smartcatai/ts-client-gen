using System;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут, которым помечаются контроллеры для форсирования маппинга каких-либо типов на тип в TypeScript.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class RequireTypeScriptTypeAttribute : Attribute
	{
		public RequireTypeScriptTypeAttribute(Type type)
		{
			GeneratedType = type;
		}
		
		public Type GeneratedType { get; }
	}
}
