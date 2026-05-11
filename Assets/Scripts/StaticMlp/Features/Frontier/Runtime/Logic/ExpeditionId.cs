using System;

namespace StaticMlp.Features.Frontier
{
    public readonly struct ExpeditionId : IEquatable<ExpeditionId>
    {
        public readonly ushort Value;

        public ExpeditionId(ushort value)
        {
            Value = value;
        }

        public bool Equals(ExpeditionId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ExpeditionId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(ExpeditionId left, ExpeditionId right) => left.Equals(right);
        public static bool operator !=(ExpeditionId left, ExpeditionId right) => !left.Equals(right);
    }
}
