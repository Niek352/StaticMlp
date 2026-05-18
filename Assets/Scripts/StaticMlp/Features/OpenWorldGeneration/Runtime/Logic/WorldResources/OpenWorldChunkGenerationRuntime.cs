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
                new WorldGenerationSeed(12345),
                WorldChunkBounds.Default,
                128f,
                -7f,
                64,
                true,
                6f);
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
