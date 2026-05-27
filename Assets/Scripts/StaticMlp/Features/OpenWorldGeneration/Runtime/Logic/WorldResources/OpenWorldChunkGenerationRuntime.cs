using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration
{
    /// <summary>
    /// Resource for storing global chunk generation runtime state (seed, bounds, chunk size, water level).
    /// </summary>
    public sealed class OpenWorldChunkGenerationRuntime : IResource, IDisposable
    {
        public readonly WorldGenerationSeed Seed;
        public readonly WorldChunkBounds Bounds;
        public readonly float ChunkWorldSize;
        public readonly float WaterLevel;
        public readonly int BaseQuadCount;
        public readonly bool AddSkirts;
        public readonly float SkirtDepth;
        public readonly LayerProcLiteRuntime LayerRuntime;

        public OpenWorldChunkGenerationRuntime(
            WorldGenerationSeed seed,
            WorldChunkBounds bounds,
            float chunkWorldSize,
            float waterLevel,
            int baseQuadCount,
            bool addSkirts,
            float skirtDepth)
        {
            Seed = seed;
            Bounds = bounds;
            ChunkWorldSize = chunkWorldSize;
            WaterLevel = waterLevel;
            BaseQuadCount = baseQuadCount;
            AddSkirts = addSkirts;
            SkirtDepth = skirtDepth;
            LayerRuntime = OpenWorldGenerationLayerCatalog.CreateRuntime(this);
        }

        public static OpenWorldChunkGenerationRuntime CreateDefault()
        {
            return new OpenWorldChunkGenerationRuntime(
                new WorldGenerationSeed(OpenWorldGenerationConfig.DEFAULT_WORLD_SEED),
                WorldChunkBounds.Default,
                OpenWorldGenerationConfig.DEFAULT_CHUNK_WORLD_SIZE,
                OpenWorldGenerationConfig.WATER_LEVEL,
                OpenWorldGenerationConfig.DEFAULT_BASE_QUAD_COUNT,
                OpenWorldGenerationConfig.DEFAULT_ADD_SKIRTS,
                OpenWorldGenerationConfig.DEFAULT_SKIRT_DEPTH);
        }

        public void CompleteScheduledJobs()
        {
            LayerRuntime.CompleteScheduledJobs();
        }

        public void Dispose()
        {
            LayerRuntime.Dispose();
        }
    }
}
