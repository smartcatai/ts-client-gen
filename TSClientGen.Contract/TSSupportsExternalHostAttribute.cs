using System;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут применяется к контроллерам и позволяет параметром конструктора api-клиента
	/// указать host, на который будут производиться вызовы api 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TSSupportsExternalHostAttribute : Attribute
	{
	}
}