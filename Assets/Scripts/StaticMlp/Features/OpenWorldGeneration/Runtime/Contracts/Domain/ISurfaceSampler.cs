namespace StaticMlp.Features.OpenWorldGeneration
{
    public interface ISurfaceSampler
    {
        SurfaceSample Sample(float worldX, float worldZ);
    }
}
