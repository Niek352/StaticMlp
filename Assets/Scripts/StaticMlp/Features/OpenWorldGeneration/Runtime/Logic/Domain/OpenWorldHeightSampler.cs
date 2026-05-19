namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldHeightSampler : IHeightSampler
    {
        private readonly uint _seed;

        public OpenWorldHeightSampler(WorldGenerationSeed seed)
        {
            _seed = (uint)seed.Value;
        }

        public float SampleHeight(float worldX, float worldZ)
        {
            return OpenWorldSurfaceRules.SampleHeight(_seed, worldX, worldZ);
        }
    }
}
