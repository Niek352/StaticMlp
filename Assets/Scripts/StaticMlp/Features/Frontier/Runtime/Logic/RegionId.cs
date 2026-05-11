using System;

namespace StaticMlp.Features.Frontier
{
    public readonly struct RegionId : IEquatable<RegionId>
    {
        public readonly ushort Value;

        public RegionId(ushort value)
        {
            Value = value;
        }

        public bool Equals(RegionId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RegionId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(RegionId left, RegionId right) => left.Equals(right);
        public static bool operator !=(RegionId left, RegionId right) => !left.Equals(right);
    }
}
