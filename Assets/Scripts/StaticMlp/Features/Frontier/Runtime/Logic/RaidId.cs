using System;

namespace StaticMlp.Features.Frontier
{
    public readonly struct RaidId : IEquatable<RaidId>
    {
        public readonly ushort Value;

        public RaidId(ushort value)
        {
            Value = value;
        }

        public bool Equals(RaidId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RaidId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(RaidId left, RaidId right) => left.Equals(right);
        public static bool operator !=(RaidId left, RaidId right) => !left.Equals(right);
    }
}
