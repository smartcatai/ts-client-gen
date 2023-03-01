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
			if (type.IsPrimitive || _standartTypes.Contains(type))
				return true;
			
			if (type.Name == "Nullable`1") 
			{
				var nullableType = type.GenericTypeArguments[0];
				return nullableType.IsSimpleType();
			}

			return false;
		}
	}
}