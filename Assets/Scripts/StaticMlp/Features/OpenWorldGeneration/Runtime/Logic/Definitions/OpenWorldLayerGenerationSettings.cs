using System;
using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldLayerGenerationSettings
    {
        public readonly uint WorldSeed;
        public readonly float ChunkWorldSize;
        public readonly float WaterLevel;
        public readonly int BaseQuadCount;
        public readonly int Lod;
        public readonly bool AddSkirts;
        public readonly float SkirtDepth;

        public OpenWorldLayerGenerationSettings(
            uint worldSeed,
            float chunkWorldSize,
            float waterLevel,
            int baseQuadCount,
            int lod,
            bool addSkirts,
            float skirtDepth)
        {
            WorldSeed = worldSeed;
            ChunkWorldSize = chunkWorldSize;
            WaterLevel = waterLevel;
            BaseQuadCount = baseQuadCount;
            Lod = lod;
            AddSkirts = addSkirts;
            SkirtDepth = skirtDepth;
        }

        public static OpenWorldLayerGenerationSettings FromContext(in LayerProcLiteScheduleContext context)
        {
            if (context.Settings is not OpenWorldLayerGenerationSettings settings)
                throw new InvalidOperationException("OpenWorld layer scheduler requires OpenWorldLayerGenerationSettings.");

            return settings;
        }
    }
}
