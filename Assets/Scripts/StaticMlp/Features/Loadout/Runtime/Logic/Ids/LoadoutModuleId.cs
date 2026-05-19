using System;

namespace StaticMlp.Features.Loadout
{
    public readonly struct LoadoutModuleId : IEquatable<LoadoutModuleId>
    {
        public readonly ushort Value;

        public LoadoutModuleId(ushort value)
        {
            Value = value;
        }

        public bool Equals(LoadoutModuleId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is LoadoutModuleId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(LoadoutModuleId left, LoadoutModuleId right) => left.Equals(right);
        public static bool operator !=(LoadoutModuleId left, LoadoutModuleId right) => !left.Equals(right);
    }
}
