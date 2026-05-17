using StaticMlp.Features.OpenWorldGeneration.Jobs;
using StaticMlp.LayerProcLite;
using Unity.Collections;
using Unity.Jobs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldPlacementLayerScheduler : ILayerProcLiteLayerScheduler
    {
        public LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context)
        {
            var settings = OpenWorldLayerGenerationSettings.FromContext(context);
            var surface = context.Providers.GetSingleOverlapping<OpenWorldSurfaceChunkData>(
                OpenWorldGenerationLayerIds.Surface,
                0,
                context.Bounds);
            var resourcePlacements = new NativeArray<ResourcePlacement>(
                ResourcePlacementGenerationJob.RESOURCE_CANDIDATES_PER_CHUNK + 1,
                Allocator.Persistent);
            var resourcePlacementCount = new NativeArray<int>(1, Allocator.Persistent);
            var spawnPlacements = new NativeArray<SpawnPlacement>(
                ResourcePlacementGenerationJob.SPAWN_CANDIDATES_PER_CHUNK + 1,
                Allocator.Persistent);
            var spawnPlacementCount = new NativeArray<int>(1, Allocator.Persistent);
            var data = new OpenWorldPlacementChunkData(
                resourcePlacements,
                resourcePlacementCount,
                spawnPlacements,
                spawnPlacementCount);
            var chunkId = new WorldChunkId(context.Key.ChunkId.X, context.Key.ChunkId.Z);
            var handle = new ResourcePlacementGenerationJob
            {
                ChunkId = chunkId,
                LayerChunkId = context.Key.ChunkId,
                WorldSeed = settings.WorldSeed,
                ChunkWorldSize = settings.ChunkWorldSize,
                Resolution = surface.Layout.OutputResolution,
                Surfaces = surface.Surfaces.AsReadOnly(),
                ResourcePlacements = resourcePlacements,
                ResourcePlacementCount = resourcePlacementCount,
                SpawnPlacements = spawnPlacements,
                SpawnPlacementCount = spawnPlacementCount
            }.Schedule(context.DependencyHandle);

            return new LayerProcLiteScheduleResult(handle, data);
        }
    }
}
