using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using TSClientGen.Extensibility;

namespace TSClientGen.AspNetWebApi
{
	public class TypeConverter : ITypeConverter
	{
		public TypeConverter(ITypeConverter next)
		{
			_next = next;
		}
		
		public string Convert(Type type, Func<Type, string> defaultConvert)
		{
			var result = _next?.Convert(type, defaultConvert);
			if (result != null)
				return result;

			if (type == typeof(HttpResponseMessage) || type == typeof(JObject) || type == typeof(JValue))
				return "any";

			return null;
		}
		
		private readonly ITypeConverter _next;		
	}
}