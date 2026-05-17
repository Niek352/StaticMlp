using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteChunkId : IEquatable<LayerProcLiteChunkId>
    {
        public readonly int X;
        public readonly int Z;

        public LayerProcLiteChunkId(int x, int z)
        {
            X = x;
            Z = z;
        }

        public bool Equals(LayerProcLiteChunkId other)
        {
            return X == other.X && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteChunkId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Z;
            }
        }

        public static bool operator ==(LayerProcLiteChunkId left, LayerProcLiteChunkId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayerProcLiteChunkId left, LayerProcLiteChunkId right)
        {
            return !left.Equals(right);
        }
    }
}
