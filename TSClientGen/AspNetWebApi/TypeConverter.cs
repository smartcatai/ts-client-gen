using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TSClientGen.AspNetWebApi
{
	public class TypeConverter : ICustomTypeConverter
	{
		public string Convert(Type type, Func<Type, string> defaultConvert)
		{
			if (type == typeof(HttpResponseMessage) || type == typeof(JObject) || type == typeof(JValue))
				return "any";

			return defaultConvert(type);
		}
	}
}