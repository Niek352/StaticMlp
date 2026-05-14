using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct WorldGenerationRequest
    {
        public readonly WorldGenerationSeed Seed;
        public readonly WorldChunkBounds Bounds;
        public readonly float ChunkWorldSize;
        public readonly int BaseQuadCount;
        public readonly int Lod;
        public readonly bool AddSkirts;
        public readonly float SkirtDepth;

        public WorldGenerationRequest(
            WorldGenerationSeed seed,
            WorldChunkBounds bounds,
            float chunkWorldSize,
            int baseQuadCount,
            int lod,
            bool addSkirts,
            float skirtDepth)
        {
            if (chunkWorldSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(chunkWorldSize), chunkWorldSize, "Chunk world size must be positive.");
            if (baseQuadCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(baseQuadCount), baseQuadCount, "Base quad count must be positive.");
            if (lod < 0)
                throw new ArgumentOutOfRangeException(nameof(lod), lod, "LOD must be non-negative.");
            if ((baseQuadCount >> lod) <= 0)
                throw new ArgumentOutOfRangeException(nameof(lod), lod, "LOD is too high for the base quad count.");
            if (addSkirts && skirtDepth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(skirtDepth), skirtDepth, "Skirt depth must be positive when skirts are enabled.");

            Seed = seed;
            Bounds = bounds;
            ChunkWorldSize = chunkWorldSize;
            BaseQuadCount = baseQuadCount;
            Lod = lod;
            AddSkirts = addSkirts;
            SkirtDepth = skirtDepth;
        }
    }
}
