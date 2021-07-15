namespace TSClientGen
{
    public readonly struct TypeMappingConfig
    {
        public static TypeMappingConfig FromArgs(Arguments args) =>
            new TypeMappingConfig(args.NullabilityHandling);

        public TypeMappingConfig(NullabilityHandling nullabilityHandling)
        {
            NullabilityHandling = nullabilityHandling;
        }

        public NullabilityHandling NullabilityHandling { get; }
    }
}