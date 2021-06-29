using System.Linq;
using System.Reflection;

namespace TSClientGen.Nullability
{
    public class DataAnnotationsNullabilityHandler : INullabilityHandler
    {
        private readonly INullabilityHandler _defaultTo;

        public DataAnnotationsNullabilityHandler(INullabilityHandler defaultTo)
        {
            _defaultTo = defaultTo;
        }

        public TsZeroType GetTsNullability(PropertyInfo property)
        {
            if (property.PropertyType.IsValueType)
                return _defaultTo.GetTsNullability(property);
            // don't reference the assembly just to get the name of an attribute preventing the dependency conflicts
            var optional = property.CustomAttributes.All(x => x.AttributeType.Name != "RequiredAttribute");
            return TsZeroType.Nullable(optional);
        }
    }
}