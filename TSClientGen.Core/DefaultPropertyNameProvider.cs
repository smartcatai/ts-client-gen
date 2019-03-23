using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TSClientGen.Extensibility;

namespace TSClientGen
{
	public sealed class DefaultPropertyNameProvider : IPropertyNameProvider
	{
		public string GetPropertyName(PropertyInfo property)
		{
			var ignoreDataMember = property.GetCustomAttributes<IgnoreDataMemberAttribute>().FirstOrDefault();
			if (ignoreDataMember != null)
				return null;

			var dataMember = property.GetCustomAttributes<DataMemberAttribute>().FirstOrDefault();
			return dataMember?.Name ?? toLowerCamelCase(property.Name);
		}
		
		private string toLowerCamelCase(string name)
		{
			return char.ToLowerInvariant(name[0]) + name.Substring(1);
		}
	}
}