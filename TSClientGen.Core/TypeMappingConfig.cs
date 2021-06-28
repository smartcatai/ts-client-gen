namespace TSClientGen
{
    public readonly struct TypeMappingConfig
    {
        public static TypeMappingConfig FromArgs(Arguments args) =>
            new TypeMappingConfig(
                args.NullabilityHandling,
                args.CheckNullabilityForOverrides,
                args.NullablePropertiesAreOptionalTooIfUnspecified);

        public TypeMappingConfig(NullabilityHandling nullabilityHandling, bool checkNullabilityForOverrides, bool nullablePropertiesAreOptionalTooIfUnspecified)
        {
            NullabilityHandling = nullabilityHandling;
            CheckNullabilityForOverrides = checkNullabilityForOverrides;
            NullablePropertiesAreOptionalTooIfUnspecified = nullablePropertiesAreOptionalTooIfUnspecified;
        }

        public NullabilityHandling NullabilityHandling { get; }
        public bool CheckNullabilityForOverrides { get; }
        public bool NullablePropertiesAreOptionalTooIfUnspecified { get; }
    }
}