using System;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct WorldChunkId : IEquatable<WorldChunkId>
    {
        public readonly int X;
        public readonly int Z;

        public WorldChunkId(int x, int z)
        {
            X = x;
            Z = z;
        }

        public static WorldChunkId FromWorldPosition(float worldX, float worldZ, float chunkWorldSize)
        {
            if (chunkWorldSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(chunkWorldSize), chunkWorldSize, "Chunk world size must be positive.");

            return new WorldChunkId(
                (int)Math.Floor(worldX / chunkWorldSize),
                (int)Math.Floor(worldZ / chunkWorldSize));
        }

        public Vector3 GetWorldOrigin(float chunkWorldSize)
        {
            return new Vector3(X * chunkWorldSize, 0f, Z * chunkWorldSize);
        }

        public Vector3 GetWorldCenter(float chunkWorldSize)
        {
            return new Vector3((X + 0.5f) * chunkWorldSize, 0f, (Z + 0.5f) * chunkWorldSize);
        }

        public bool Equals(WorldChunkId other)
        {
            return X == other.X && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldChunkId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Z;
            }
        }

        public override string ToString()
        {
            return $"({X},{Z})";
        }

        public static bool operator ==(WorldChunkId left, WorldChunkId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldChunkId left, WorldChunkId right)
        {
            return !left.Equals(right);
        }
    }
}
