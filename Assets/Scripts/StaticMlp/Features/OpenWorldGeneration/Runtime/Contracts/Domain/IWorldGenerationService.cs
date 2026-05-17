using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    [Obsolete("Replaced by OpenWorldChunkGenerationSystem")]
    public interface IWorldGenerationService
    {
        GeneratedChunkData GenerateChunk(WorldChunkId chunkId, WorldGenerationRequest request);
    }
}
