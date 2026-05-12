using System;

namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingId : IEquatable<BuildingId>
    {
        public readonly ushort Value;

        public BuildingId(ushort value)
        {
            Value = value;
        }

        public bool Equals(BuildingId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is BuildingId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(BuildingId left, BuildingId right) => left.Equals(right);
        public static bool operator !=(BuildingId left, BuildingId right) => !left.Equals(right);
    }
}
