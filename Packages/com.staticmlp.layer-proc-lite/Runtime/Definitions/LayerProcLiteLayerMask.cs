using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteLayerMask : IEquatable<LayerProcLiteLayerMask>
    {
        public static readonly LayerProcLiteLayerMask None = new(0UL);

        public readonly ulong Value;

        public LayerProcLiteLayerMask(ulong value)
        {
            Value = value;
        }

        public bool IsEmpty => Value == 0UL;

        public bool Contains(LayerProcLiteLayerId layerId)
        {
            return (Value & Bit(layerId)) != 0UL;
        }

        public LayerProcLiteLayerMask With(LayerProcLiteLayerId layerId)
        {
            return new LayerProcLiteLayerMask(Value | Bit(layerId));
        }

        public LayerProcLiteLayerMask Without(LayerProcLiteLayerId layerId)
        {
            return new LayerProcLiteLayerMask(Value & ~Bit(layerId));
        }

        public bool Equals(LayerProcLiteLayerMask other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteLayerMask other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static LayerProcLiteLayerMask From(LayerProcLiteLayerId layerId)
        {
            return new LayerProcLiteLayerMask(Bit(layerId));
        }

        public static LayerProcLiteLayerMask operator |(LayerProcLiteLayerMask left, LayerProcLiteLayerMask right)
        {
            return new LayerProcLiteLayerMask(left.Value | right.Value);
        }

        public static LayerProcLiteLayerMask operator &(LayerProcLiteLayerMask left, LayerProcLiteLayerMask right)
        {
            return new LayerProcLiteLayerMask(left.Value & right.Value);
        }

        public static bool operator ==(LayerProcLiteLayerMask left, LayerProcLiteLayerMask right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayerProcLiteLayerMask left, LayerProcLiteLayerMask right)
        {
            return !left.Equals(right);
        }

        private static ulong Bit(LayerProcLiteLayerId layerId)
        {
            return 1UL << layerId.Value;
        }
    }
}
