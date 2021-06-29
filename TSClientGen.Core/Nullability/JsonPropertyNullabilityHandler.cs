using System.Reflection;

namespace TSClientGen.Nullability
{
    public class JsonPropertyNullabilityHandler : INullabilityHandler
    {
        private readonly INullabilityHandler _defaultTo;

        public JsonPropertyNullabilityHandler(INullabilityHandler defaultTo)
        {
            _defaultTo = defaultTo;
        }

        public TsZeroType GetTsNullability(PropertyInfo property)
        {
            var attr = JsonPropertyExtractor.Extract(property);
            var req = attr?.Required();
            if (req.HasValue)
                return req.Value;
            return _defaultTo.GetTsNullability(property);
        }
    }
}