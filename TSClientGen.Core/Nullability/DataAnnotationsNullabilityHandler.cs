using System.Linq;
using System.Reflection;

namespace TSClientGen.Nullability
{
    public class DataAnnotationsNullabilityHandler : INullabilityHandler
    {
        public TsZeroType GetTsNullability(PropertyInfo property)
        {
            // don't reference the assembly just to get the name of an attribute preventing the dependency conflicts
            var optional = property.CustomAttributes.All(x => x.AttributeType.Name != "RequiredAttribute");
            return TsZeroType.Optional(optional);
        }
    }
}