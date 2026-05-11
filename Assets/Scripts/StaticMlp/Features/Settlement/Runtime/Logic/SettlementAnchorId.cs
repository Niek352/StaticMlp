using System;

namespace StaticMlp.Features.Settlement
{
    public readonly struct SettlementAnchorId : IEquatable<SettlementAnchorId>
    {
        public readonly ushort Value;

        public SettlementAnchorId(ushort value)
        {
            Value = value;
        }

        public bool Equals(SettlementAnchorId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SettlementAnchorId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(SettlementAnchorId left, SettlementAnchorId right) => left.Equals(right);
        public static bool operator !=(SettlementAnchorId left, SettlementAnchorId right) => !left.Equals(right);
    }
}
