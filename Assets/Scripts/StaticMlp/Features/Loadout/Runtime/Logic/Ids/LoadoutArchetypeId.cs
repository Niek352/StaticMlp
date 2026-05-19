using System;

namespace StaticMlp.Features.Loadout
{
    public readonly struct LoadoutArchetypeId : IEquatable<LoadoutArchetypeId>
    {
        public readonly ushort Value;

        public LoadoutArchetypeId(ushort value)
        {
            Value = value;
        }

        public bool Equals(LoadoutArchetypeId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is LoadoutArchetypeId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(LoadoutArchetypeId left, LoadoutArchetypeId right) => left.Equals(right);
        public static bool operator !=(LoadoutArchetypeId left, LoadoutArchetypeId right) => !left.Equals(right);
    }
}
