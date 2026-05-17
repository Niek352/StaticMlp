using System;
using Unity.Mathematics;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteWorldBounds : IEquatable<LayerProcLiteWorldBounds>
    {
        public readonly float MinX;
        public readonly float MinZ;
        public readonly float MaxX;
        public readonly float MaxZ;

        public LayerProcLiteWorldBounds(float minX, float minZ, float maxX, float maxZ)
        {
            if (maxX <= minX)
                throw new ArgumentOutOfRangeException(nameof(maxX), maxX, "Bounds max X must be greater than min X.");
            if (maxZ <= minZ)
                throw new ArgumentOutOfRangeException(nameof(maxZ), maxZ, "Bounds max Z must be greater than min Z.");

            MinX = minX;
            MinZ = minZ;
            MaxX = maxX;
            MaxZ = maxZ;
        }

        public static LayerProcLiteWorldBounds FromChunk(LayerProcLiteChunkId chunkId, float chunkWorldSize)
        {
            if (chunkWorldSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(chunkWorldSize), chunkWorldSize, "Chunk world size must be positive.");

            float minX = chunkId.X * chunkWorldSize;
            float minZ = chunkId.Z * chunkWorldSize;
            return new LayerProcLiteWorldBounds(minX, minZ, minX + chunkWorldSize, minZ + chunkWorldSize);
        }

        public LayerProcLiteWorldBounds Expanded(float paddingWorld)
        {
            if (paddingWorld < 0f)
                throw new ArgumentOutOfRangeException(nameof(paddingWorld), paddingWorld, "Padding must be non-negative.");

            return new LayerProcLiteWorldBounds(
                MinX - paddingWorld,
                MinZ - paddingWorld,
                MaxX + paddingWorld,
                MaxZ + paddingWorld);
        }

        public bool Contains(in LayerProcLiteWorldBounds other)
        {
            return other.MinX >= MinX
                   && other.MinZ >= MinZ
                   && other.MaxX <= MaxX
                   && other.MaxZ <= MaxZ;
        }

        public bool Overlaps(in LayerProcLiteWorldBounds other)
        {
            return MinX < other.MaxX
                   && MaxX > other.MinX
                   && MinZ < other.MaxZ
                   && MaxZ > other.MinZ;
        }

        public bool Equals(LayerProcLiteWorldBounds other)
        {
            return MinX.Equals(other.MinX)
                   && MinZ.Equals(other.MinZ)
                   && MaxX.Equals(other.MaxX)
                   && MaxZ.Equals(other.MaxZ);
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteWorldBounds other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = MinX.GetHashCode();
                hash = (hash * 397) ^ MinZ.GetHashCode();
                hash = (hash * 397) ^ MaxX.GetHashCode();
                hash = (hash * 397) ^ MaxZ.GetHashCode();
                return hash;
            }
        }

        public int2 MinChunk(float chunkWorldSize)
        {
            ValidateChunkWorldSize(chunkWorldSize);
            return new int2(
                (int)math.floor(MinX / chunkWorldSize),
                (int)math.floor(MinZ / chunkWorldSize));
        }

        public int2 MaxChunk(float chunkWorldSize)
        {
            ValidateChunkWorldSize(chunkWorldSize);
            return new int2(
                (int)math.ceil(MaxX / chunkWorldSize) - 1,
                (int)math.ceil(MaxZ / chunkWorldSize) - 1);
        }

        private static void ValidateChunkWorldSize(float chunkWorldSize)
        {
            if (chunkWorldSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(chunkWorldSize), chunkWorldSize, "Chunk world size must be positive.");
        }
    }
}
