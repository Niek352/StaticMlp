using System;
using Runevision.Common;
using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgBiomeLayer : ChunkBasedDataLayer<LpgBiomeLayer, LpgBiomeChunk>
    {
        public override int chunkW => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;
        public override int chunkH => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;

        public LayerProcGenWorldContext Context => LayerProcGenWorldGenerationService.Context;

        public LpgBiomeLayer()
        {
            AddLayerDependency(new LayerDependency(LpgRegionLayer.instance, 0));
        }

        public byte GetBiomeId(ILC requester, float worldX, float worldZ, float height, float waterMask)
        {
            if (waterMask > 0.5f)
                return 4;
            if (height > 14f)
                return 3;

            var bounds = new GridBounds((int)Math.Floor(worldX), (int)Math.Floor(worldZ), 1, 1);
            byte moistureBand = 0;
            HandleChunksInBounds(requester, bounds, 0, chunk => moistureBand = chunk.MoistureBand);
            var regionId = LpgRegionLayer.instance.GetRegionId(requester, worldX, worldZ);
            return (byte)((moistureBand + regionId) % 2 == 0 ? 1 : 2);
        }
    }
}
