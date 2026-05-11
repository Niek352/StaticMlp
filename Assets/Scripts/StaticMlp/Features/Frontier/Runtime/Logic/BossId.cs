using System;

namespace StaticMlp.Features.Frontier
{
    public readonly struct BossId : IEquatable<BossId>
    {
        public readonly ushort Value;

        public BossId(ushort value)
        {
            Value = value;
        }

        public bool Equals(BossId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is BossId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(BossId left, BossId right) => left.Equals(right);
        public static bool operator !=(BossId left, BossId right) => !left.Equals(right);
    }
}
