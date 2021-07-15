using System.Reflection;

namespace TSClientGen.Nullability
{
    public interface INullabilityHandler
    {
        TsZeroType GetTsNullability(PropertyInfo property);
    }
}