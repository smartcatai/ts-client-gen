using System;

namespace TSClientGen.Nullability
{
    public readonly struct TsZeroType : IEquatable<TsZeroType>
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

        public override string ToString() =>
            (IsNullable, IsOptional) switch
            {
                (true, true) => "[nullable, optional]",
                (true, false) => "[nullable]",
                (false, true) => "[optional]",
                (false, false) => "[no zero type values allowed]"
            };

        public bool Equals(TsZeroType other)
        {
            return IsNullable == other.IsNullable && IsOptional == other.IsOptional;
        }

        public override bool Equals(object obj)
        {
            return obj is TsZeroType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IsNullable.GetHashCode() * 397) ^ IsOptional.GetHashCode();
            }
        }

        public static bool operator ==(TsZeroType left, TsZeroType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TsZeroType left, TsZeroType right)
        {
            return !left.Equals(right);
        }
    }
}