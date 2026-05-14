using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgRegionChunk : LayerChunk<LpgRegionLayer, LpgRegionChunk>
    {
        public byte RegionId;

        public override void Create(int level, bool destroy)
        {
            if (destroy)
            {
                RegionId = 0;
                return;
            }

            RegionId = (byte)(1 + layer.Context.RegionRandom.Range(0, 4, index.x, index.y));
        }
    }
}
