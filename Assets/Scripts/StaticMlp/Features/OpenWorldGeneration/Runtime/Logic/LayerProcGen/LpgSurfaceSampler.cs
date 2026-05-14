using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgSurfaceSampler : ISurfaceSampler
    {
        private readonly ILC _requester;

        public LpgSurfaceSampler(ILC requester)
        {
            _requester = requester;
        }

        public SurfaceSample Sample(float worldX, float worldZ)
        {
            return LpgSurfaceLayer.instance.Sample(_requester, worldX, worldZ);
        }
    }
}
