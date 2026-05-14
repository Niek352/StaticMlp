using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct WorldChunkBounds : IEquatable<WorldChunkBounds>
    {
        public readonly int MinX;
        public readonly int MaxX;
        public readonly int MinZ;
        public readonly int MaxZ;

        public WorldChunkBounds(int minX, int maxX, int minZ, int maxZ)
        {
            if (minX > maxX)
                throw new ArgumentException("Min X must be less than or equal to max X.", nameof(minX));
            if (minZ > maxZ)
                throw new ArgumentException("Min Z must be less than or equal to max Z.", nameof(minZ));

            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public static WorldChunkBounds Default => new(-8, 7, -8, 7);

        public bool Contains(WorldChunkId chunkId)
        {
            return chunkId.X >= MinX
                   && chunkId.X <= MaxX
                   && chunkId.Z >= MinZ
                   && chunkId.Z <= MaxZ;
        }

        public WorldChunkId Clamp(WorldChunkId chunkId)
        {
            return new WorldChunkId(
                Math.Min(Math.Max(chunkId.X, MinX), MaxX),
                Math.Min(Math.Max(chunkId.Z, MinZ), MaxZ));
        }

        public bool Equals(WorldChunkBounds other)
        {
            return MinX == other.MinX
                   && MaxX == other.MaxX
                   && MinZ == other.MinZ
                   && MaxZ == other.MaxZ;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldChunkBounds other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = MinX;
                hash = (hash * 397) ^ MaxX;
                hash = (hash * 397) ^ MinZ;
                hash = (hash * 397) ^ MaxZ;
                return hash;
            }
        }
    }
}
