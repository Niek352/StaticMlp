using System;

namespace StaticMlp.Features.Build
{
    public readonly struct BuildArchetypeId : IEquatable<BuildArchetypeId>
    {
        public readonly ushort Value;

        public BuildArchetypeId(ushort value)
        {
            Value = value;
        }

        public bool Equals(BuildArchetypeId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is BuildArchetypeId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(BuildArchetypeId left, BuildArchetypeId right) => left.Equals(right);
        public static bool operator !=(BuildArchetypeId left, BuildArchetypeId right) => !left.Equals(right);
    }
}
