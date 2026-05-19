using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public interface IHeightSampler : IResource
    {
        float SampleHeight(float worldX, float worldZ);
    }
}
