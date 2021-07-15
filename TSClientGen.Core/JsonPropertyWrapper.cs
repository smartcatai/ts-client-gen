using System;
using System.Reflection;
using TSClientGen.Nullability;

namespace TSClientGen
{
    public struct JsonPropertyWrapper
    {
        private readonly Attribute _attributeValue;

        public JsonPropertyWrapper(Attribute attributeValue)
        {
            _attributeValue = attributeValue;
        }

        public TsZeroType? Required()
        {
            var required =
                _attributeValue
                    ?.GetType()
                    .GetProperty("Required", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(_attributeValue);
            if (required == null)
                return null;
            int requiredValue;
            try
            {
                requiredValue = (int) required;
            }
            catch (InvalidCastException)
            {
                return null;
            }

            // TODO: consider handling of NullValueHandling and DefaultValueHandling enums
            // below are values of Newtonsoft.Json.Required attribute.
            // They may change between versions of course, but I believe there's a really small chance of this to happen
            // The property is not required. The default state.
            const int @default = 0;
            // The property must be defined in JSON but can be a null value.
            const int allowNull = 1;
            // The property must be defined in JSON and cannot be a null value.
            const int always = 2;
            // The property is not required but it cannot be a null value.
            const int disallowNull = 3;
            return requiredValue switch
            {
                @default => TsZeroType.OptionalNullable() /* if jsonAttribute, then jsonAttribute all the way. So Default allows nulls for value types too */,
                allowNull => TsZeroType.Nullable(),
                always => TsZeroType.None(),
                disallowNull => TsZeroType.Optional(),
                { } unknown => throw new InvalidOperationException($"Unknown value {unknown} for Newtonsoft.Json.Required enum")
            };
        }
        
    }
}