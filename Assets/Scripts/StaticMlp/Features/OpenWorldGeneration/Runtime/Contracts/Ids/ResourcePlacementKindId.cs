using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct ResourcePlacementKindId : IEquatable<ResourcePlacementKindId>
    {
        public readonly ushort Value;

        public ResourcePlacementKindId(ushort value)
        {
            Value = value;
        }

        public bool Equals(ResourcePlacementKindId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ResourcePlacementKindId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(ResourcePlacementKindId left, ResourcePlacementKindId right) => left.Equals(right);
        public static bool operator !=(ResourcePlacementKindId left, ResourcePlacementKindId right) => !left.Equals(right);
    }
}
