namespace StaticMlp.Features.OpenWorldGeneration
{
    public interface IWorldGenerationService
    {
        GeneratedChunkData GenerateChunk(WorldChunkId chunkId, WorldGenerationRequest request);
    }
}
