using System.Linq;
using System.Reflection;

namespace TSClientGen
{
    public static class JsonPropertyExtractor
    {
        public static JsonPropertyWrapper? Extract(PropertyInfo property)
        {
            var attr = property.GetCustomAttributes();
            // don't reference the assembly just to get the name of an attribute preventing the dependency conflicts
            var jsonPropertyAttribute =
                attr.SingleOrDefault(x => x.GetType().Name == "JsonPropertyAttribute");
            if (jsonPropertyAttribute == null)
                return null;
            return new JsonPropertyWrapper(jsonPropertyAttribute);
        }
    }
}