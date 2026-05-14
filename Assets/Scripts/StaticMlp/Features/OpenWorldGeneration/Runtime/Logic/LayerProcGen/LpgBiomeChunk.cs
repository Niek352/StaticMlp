using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgBiomeChunk : LayerChunk<LpgBiomeLayer, LpgBiomeChunk>
    {
        public byte MoistureBand;

        public override void Create(int level, bool destroy)
        {
            if (destroy)
            {
                MoistureBand = 0;
                return;
            }

            MoistureBand = (byte)layer.Context.BiomeRandom.Range(0, 3, index.x, index.y);
        }
    }
}
