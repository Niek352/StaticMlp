using System;

namespace StaticMlp.Features.Progression
{
    public readonly struct RewardPackageId : IEquatable<RewardPackageId>
    {
        public readonly ushort Value;

        public RewardPackageId(ushort value)
        {
            Value = value;
        }

        public bool Equals(RewardPackageId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RewardPackageId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(RewardPackageId left, RewardPackageId right) => left.Equals(right);
        public static bool operator !=(RewardPackageId left, RewardPackageId right) => !left.Equals(right);
    }
}
