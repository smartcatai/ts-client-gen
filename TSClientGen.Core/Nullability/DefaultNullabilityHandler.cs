using System;
using System.Reflection;

namespace TSClientGen.Nullability
{
    public class DefaultNullabilityHandler : INullabilityHandler
    {
        public TsZeroType GetTsNullability(PropertyInfo property) =>
            TsZeroType.Optional(
                property.PropertyType.IsValueType &&
                Nullable.GetUnderlyingType(property.PropertyType) != null);
    }
}