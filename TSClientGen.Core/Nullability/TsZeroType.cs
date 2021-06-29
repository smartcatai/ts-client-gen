namespace TSClientGen.Nullability
{
    public struct TsZeroType
    {
        public static TsZeroType None() => new TsZeroType(nullable: false, optional: false);
        public static TsZeroType Nullable(bool nullable = true) => new TsZeroType(nullable, optional: false);
        public static TsZeroType Optional(bool optional = true) => new TsZeroType(nullable: false, optional);
        public static TsZeroType OptionalNullable(bool nullable = true, bool optional = true) => new TsZeroType(nullable, optional);
        
        private TsZeroType(bool nullable, bool optional)
        {
            IsNullable = nullable;
            IsOptional = optional;
        }

        public bool IsNullable { get; }
        public bool IsOptional { get; }
    }
}