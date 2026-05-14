using StaticMlp.Features.OpenWorldGeneration;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public readonly struct OpenWorldResourceNodeSpawnSpec
    {
        public OpenWorldResourceNodeSpawnSpec(
            long placementId,
            ResourcePlacementKindId kindId,
            WorldChunkId chunkId,
            Vector3 position,
            float yawDegrees,
            float scale,
            int remainingAmount)
        {
            PlacementId = placementId;
            KindId = kindId;
            ChunkId = chunkId;
            Position = position;
            YawDegrees = yawDegrees;
            Scale = scale;
            RemainingAmount = remainingAmount;
        }

        public readonly long PlacementId;
        public readonly ResourcePlacementKindId KindId;
        public readonly WorldChunkId ChunkId;
        public readonly Vector3 Position;
        public readonly float YawDegrees;
        public readonly float Scale;
        public readonly int RemainingAmount;
    }
}
