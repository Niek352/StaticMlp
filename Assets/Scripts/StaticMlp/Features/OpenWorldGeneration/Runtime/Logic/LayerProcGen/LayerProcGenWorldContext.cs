using Runevision.Common;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LayerProcGenWorldContext
    {
        public LayerProcGenWorldContext(LayerProcGenWorldSettings settings)
        {
            Settings = settings;
            RegionRandom = new RandomHash(settings.Seed.Value ^ 0x2D4C1B);
            BiomeRandom = new RandomHash(settings.Seed.Value ^ 0x515EED);
        }

        public readonly LayerProcGenWorldSettings Settings;
        public readonly RandomHash RegionRandom;
        public readonly RandomHash BiomeRandom;
    }
}
