using System;
using Runevision.Common;
using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgRegionLayer : ChunkBasedDataLayer<LpgRegionLayer, LpgRegionChunk>
    {
        public override int chunkW => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize * 4;
        public override int chunkH => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize * 4;

        public LayerProcGenWorldContext Context => LayerProcGenWorldGenerationService.Context;

        public byte GetRegionId(ILC requester, float worldX, float worldZ)
        {
            var bounds = new GridBounds((int)Math.Floor(worldX), (int)Math.Floor(worldZ), 1, 1);
            byte regionId = 0;
            HandleChunksInBounds(requester, bounds, 0, chunk => regionId = chunk.RegionId);
            return regionId;
        }
    }
}
