using System;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct SpawnPlacement : IEquatable<SpawnPlacement>
    {
        public readonly SpawnPlacementKindId KindId;
        public readonly WorldChunkId ChunkId;
        public readonly Vector3 Position;
        public readonly float YawDegrees;
        public readonly float Scale;

        public SpawnPlacement(
            SpawnPlacementKindId kindId,
            WorldChunkId chunkId,
            Vector3 position,
            float yawDegrees,
            float scale)
        {
            KindId = kindId;
            ChunkId = chunkId;
            Position = position;
            YawDegrees = yawDegrees;
            Scale = scale;
        }

        public bool Equals(SpawnPlacement other)
        {
            return KindId == other.KindId
                   && ChunkId == other.ChunkId
                   && Position == other.Position
                   && YawDegrees.Equals(other.YawDegrees)
                   && Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj) => obj is SpawnPlacement other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = KindId.GetHashCode();
                hash = (hash * 397) ^ ChunkId.GetHashCode();
                hash = (hash * 397) ^ Position.GetHashCode();
                hash = (hash * 397) ^ YawDegrees.GetHashCode();
                hash = (hash * 397) ^ Scale.GetHashCode();
                return hash;
            }
        }
    }
}
