using StaticMlp.LayerProcLite;
using Unity.Collections;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldPlacementChunkData : ILayerProcLiteChunkData
    {
        public readonly NativeArray<ResourcePlacement> ResourcePlacements;
        public readonly NativeArray<int> ResourcePlacementCount;
        public readonly NativeArray<SpawnPlacement> SpawnPlacements;
        public readonly NativeArray<int> SpawnPlacementCount;

        public OpenWorldPlacementChunkData(
            NativeArray<ResourcePlacement> resourcePlacements,
            NativeArray<int> resourcePlacementCount,
            NativeArray<SpawnPlacement> spawnPlacements,
            NativeArray<int> spawnPlacementCount)
        {
            ResourcePlacements = resourcePlacements;
            ResourcePlacementCount = resourcePlacementCount;
            SpawnPlacements = spawnPlacements;
            SpawnPlacementCount = spawnPlacementCount;
        }

        public void Dispose()
        {
            ResourcePlacements.Dispose();
            ResourcePlacementCount.Dispose();
            SpawnPlacements.Dispose();
            SpawnPlacementCount.Dispose();
        }
    }
}
