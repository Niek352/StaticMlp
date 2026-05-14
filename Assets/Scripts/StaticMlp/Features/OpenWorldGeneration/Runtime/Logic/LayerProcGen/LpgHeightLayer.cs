using System;
using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgHeightLayer : ChunkBasedDataLayer<LpgHeightLayer, LpgHeightChunk>
    {
        private const float WATER_LEVEL = -7f;

        public override int chunkW => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;
        public override int chunkH => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;

        public LayerProcGenWorldContext Context => LayerProcGenWorldGenerationService.Context;
        public float WaterLevel => WATER_LEVEL;

        public float SampleHeight(ILC requester, float worldX, float worldZ)
        {
            var seed = Context.Settings.Seed.Value;
            var low = Math.Sin((worldX + seed * 17.13f) * 0.0065f) * Math.Cos((worldZ - seed * 9.71f) * 0.0065f) * 24f;
            var mid = Math.Sin((worldX - seed * 3.37f) * 0.021f + (worldZ + seed * 2.11f) * 0.008f) * 7f;
            var ridgeWave = Math.Sin((worldX + seed) * 0.018f) * Math.Cos((worldZ - seed) * 0.015f);
            var ridge = ridgeWave * ridgeWave * 8f;
            return (float)(low + mid + ridge - 9f);
        }
    }
}
