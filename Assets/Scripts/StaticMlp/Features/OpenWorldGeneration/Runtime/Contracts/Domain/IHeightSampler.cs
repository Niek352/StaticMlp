namespace StaticMlp.Features.OpenWorldGeneration
{
    public interface IHeightSampler
    {
        float SampleHeight(float worldX, float worldZ);
    }
}
