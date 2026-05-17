using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldChunkOverlayAck : IEvent
    {
        public WorldChunkId ChunkId;
        public uint Revision;
    }
}
