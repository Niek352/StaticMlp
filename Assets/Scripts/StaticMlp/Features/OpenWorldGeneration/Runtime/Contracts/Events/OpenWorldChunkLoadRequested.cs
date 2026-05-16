using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct OpenWorldChunkLoadRequested : IEvent
    {
        public readonly WorldChunkId ChunkId;
        public readonly ushort ClusterId;

        public OpenWorldChunkLoadRequested(WorldChunkId chunkId, ushort clusterId)
        {
            ChunkId = chunkId;
            ClusterId = clusterId;
        }
    }
}
