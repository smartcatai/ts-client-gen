using System;

namespace TSClientGen.Nullability
{
    public static class NullabilityHandlerResolver
    {
        public static INullabilityHandler FromConfig(TypeMappingConfig cfg)
        {
            INullabilityHandler defaultAnythingTo = new DefaultNullabilityHandler();
            var rawHandler =
                cfg.NullabilityHandling switch
                {
                    NullabilityHandling.Default => defaultAnythingTo,
                    NullabilityHandling.Nrt => new NrtNullabilityHandler(defaultAnythingTo),
                    NullabilityHandling.DataAnnotations => new DataAnnotationsNullabilityHandler(defaultAnythingTo),
                    NullabilityHandling.JsonProperty => new JsonPropertyNullabilityHandler(defaultAnythingTo),
                    _ => throw new InvalidOperationException(
                        $"Unknown value {cfg.NullabilityHandling} for {typeof(NullabilityHandling).FullName} enum")
                };
            return rawHandler;
        }
    }
}