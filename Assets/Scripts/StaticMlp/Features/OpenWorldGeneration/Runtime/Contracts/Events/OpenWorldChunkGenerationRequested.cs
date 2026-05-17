using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    /// <summary>
    /// Request for chunk generation. Published by streaming/interest systems.
    /// </summary>
    public readonly struct OpenWorldChunkGenerationRequested : IEvent
    {
        public readonly WorldChunkId ChunkId;
        public readonly WorldGenerationRequest Request;
        public readonly GenerationOutputMask Outputs;

        public OpenWorldChunkGenerationRequested(WorldChunkId chunkId, WorldGenerationRequest request, GenerationOutputMask outputs)
        {
            ChunkId = chunkId;
            Request = request;
            Outputs = outputs;
        }
    }
}
