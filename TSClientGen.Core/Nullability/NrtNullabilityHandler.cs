using System;
using System.Reflection;
using Namotion.Reflection;

namespace TSClientGen.Nullability
{
    public class NrtNullabilityHandler : INullabilityHandler
    {
        private readonly INullabilityHandler _defaultTo;

        public NrtNullabilityHandler(INullabilityHandler defaultTo)
        {
            _defaultTo = defaultTo;
        }
        
        public TsZeroType GetTsNullability(PropertyInfo property)
        {
            if (property.PropertyType.IsValueType)
                return _defaultTo.GetTsNullability(property);
            var nullability = property.ToContextualProperty().Nullability;
            return nullability switch
            {
                Namotion.Reflection.Nullability.NotNullable => TsZeroType.None(),
                Namotion.Reflection.Nullability.Nullable => TsZeroType.Nullable(),
                Namotion.Reflection.Nullability.Unknown => _defaultTo.GetTsNullability(property),
                { } unknown => throw new InvalidOperationException(
                    $"Unknown value {unknown} for type {typeof(Namotion.Reflection.Nullability).FullName}")
            };
        }
    }
}