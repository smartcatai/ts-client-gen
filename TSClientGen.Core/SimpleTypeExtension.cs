using System;
using System.Collections.Generic;

namespace TSClientGen
{
	public static class SimpleTypeExtension
	{
		private static readonly HashSet<Type> _standartTypes = new HashSet<Type>
		{
			typeof(string),
			typeof(decimal),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(TimeSpan),
			typeof(Guid)
		};
		
		public static bool IsSimpleType(
			this Type type)
		{
			return type.IsPrimitive || _standartTypes.Contains(type);
		}
	}
}