using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteLayerId : IEquatable<LayerProcLiteLayerId>
    {
        public readonly int Value;

        public LayerProcLiteLayerId(int value)
        {
            if (value < 0 || value >= 64)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Layer id must be in range [0, 63].");

            Value = value;
        }

        public bool Equals(LayerProcLiteLayerId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteLayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(LayerProcLiteLayerId left, LayerProcLiteLayerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayerProcLiteLayerId left, LayerProcLiteLayerId right)
        {
            return !left.Equals(right);
        }
    }
}
