using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct SpawnPlacementKindId : IEquatable<SpawnPlacementKindId>
    {
        public readonly ushort Value;

        public SpawnPlacementKindId(ushort value)
        {
            Value = value;
        }

        public bool Equals(SpawnPlacementKindId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SpawnPlacementKindId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(SpawnPlacementKindId left, SpawnPlacementKindId right) => left.Equals(right);
        public static bool operator !=(SpawnPlacementKindId left, SpawnPlacementKindId right) => !left.Equals(right);
    }
}
