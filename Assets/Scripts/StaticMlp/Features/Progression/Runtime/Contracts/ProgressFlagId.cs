using System;

namespace StaticMlp.Features.Progression
{
    public readonly struct ProgressFlagId : IEquatable<ProgressFlagId>
    {
        public readonly ushort Value;

        public ProgressFlagId(ushort value)
        {
            Value = value;
        }

        public bool Equals(ProgressFlagId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ProgressFlagId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(ProgressFlagId left, ProgressFlagId right) => left.Equals(right);
        public static bool operator !=(ProgressFlagId left, ProgressFlagId right) => !left.Equals(right);
    }
}
