using System;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct ResourcePlacement : IEquatable<ResourcePlacement>
    {
        public readonly long PlacementId;
        public readonly ResourcePlacementKindId KindId;
        public readonly WorldChunkId ChunkId;
        public readonly Vector3 Position;
        public readonly float YawDegrees;
        public readonly float Scale;

        public ResourcePlacement(
            ResourcePlacementKindId kindId,
            WorldChunkId chunkId,
            Vector3 position,
            float yawDegrees,
            float scale)
            : this(CreateCompatibilityPlacementId(kindId, chunkId, position, yawDegrees, scale), kindId, chunkId, position, yawDegrees, scale)
        {
        }

        public ResourcePlacement(
            long placementId,
            ResourcePlacementKindId kindId,
            WorldChunkId chunkId,
            Vector3 position,
            float yawDegrees,
            float scale)
        {
            PlacementId = placementId == 0 ? 1 : placementId;
            KindId = kindId;
            ChunkId = chunkId;
            Position = position;
            YawDegrees = yawDegrees;
            Scale = scale;
        }

        public bool Equals(ResourcePlacement other)
        {
            return PlacementId == other.PlacementId
                   && KindId == other.KindId
                   && ChunkId == other.ChunkId
                   && Position == other.Position
                   && YawDegrees.Equals(other.YawDegrees)
                   && Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj) => obj is ResourcePlacement other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = PlacementId.GetHashCode();
                hash = (hash * 397) ^ KindId.GetHashCode();
                hash = (hash * 397) ^ ChunkId.GetHashCode();
                hash = (hash * 397) ^ Position.GetHashCode();
                hash = (hash * 397) ^ YawDegrees.GetHashCode();
                hash = (hash * 397) ^ Scale.GetHashCode();
                return hash;
            }
        }

        private static long CreateCompatibilityPlacementId(
            ResourcePlacementKindId kindId,
            WorldChunkId chunkId,
            Vector3 position,
            float yawDegrees,
            float scale)
        {
            unchecked
            {
                var hash = 1469598103934665603L;
                hash = Mix(hash, kindId.Value);
                hash = Mix(hash, chunkId.X);
                hash = Mix(hash, chunkId.Z);
                hash = Mix(hash, Mathf.RoundToInt(position.x * 1000f));
                hash = Mix(hash, Mathf.RoundToInt(position.y * 1000f));
                hash = Mix(hash, Mathf.RoundToInt(position.z * 1000f));
                hash = Mix(hash, Mathf.RoundToInt(yawDegrees * 1000f));
                hash = Mix(hash, Mathf.RoundToInt(scale * 1000f));
                hash &= long.MaxValue;
                return hash == 0 ? 1 : hash;
            }
        }

        private static long Mix(long hash, int value)
        {
            unchecked
            {
                hash ^= value;
                hash *= 1099511628211L;
                return hash;
            }
        }
    }
}
