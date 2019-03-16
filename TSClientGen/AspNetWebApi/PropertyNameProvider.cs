using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace TSClientGen.AspNetWebApi
{
	public class PropertyNameProvider : IPropertyNameProvider
	{
		public string GetPropertyName(PropertyInfo property)
		{
			var jsonProperty = property.GetCustomAttributes<JsonPropertyAttribute>().FirstOrDefault();
			return jsonProperty?.PropertyName ?? _default.GetPropertyName(property);
		}
		
		private static readonly IPropertyNameProvider _default = new DefaultPropertyNameProvider();
	}
}