using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteTopDependencyId : IEquatable<LayerProcLiteTopDependencyId>
    {
        public readonly int Value;

        public LayerProcLiteTopDependencyId(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Top dependency id must be positive.");

            Value = value;
        }

        public bool Equals(LayerProcLiteTopDependencyId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteTopDependencyId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(LayerProcLiteTopDependencyId left, LayerProcLiteTopDependencyId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayerProcLiteTopDependencyId left, LayerProcLiteTopDependencyId right)
        {
            return !left.Equals(right);
        }
    }
}
