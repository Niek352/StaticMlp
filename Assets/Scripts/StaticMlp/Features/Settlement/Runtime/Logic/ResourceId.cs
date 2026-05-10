using System;

namespace StaticMlp.Features.Settlement
{
    public readonly struct ResourceId : IEquatable<ResourceId>
    {
        public readonly ushort Value;

        public ResourceId(ushort value)
        {
            Value = value;
        }

        public bool Equals(ResourceId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ResourceId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(ResourceId left, ResourceId right) => left.Equals(right);
        public static bool operator !=(ResourceId left, ResourceId right) => !left.Equals(right);
    }
}
