using System;

namespace StaticMlp.Features.Build
{
    public readonly struct BuildModuleId : IEquatable<BuildModuleId>
    {
        public readonly ushort Value;

        public BuildModuleId(ushort value)
        {
            Value = value;
        }

        public bool Equals(BuildModuleId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is BuildModuleId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(BuildModuleId left, BuildModuleId right) => left.Equals(right);
        public static bool operator !=(BuildModuleId left, BuildModuleId right) => !left.Equals(right);
    }
}
