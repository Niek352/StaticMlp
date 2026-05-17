using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldChunkOverlayRequest : IEvent
    {
        public WorldChunkId ChunkId;
        public uint KnownRevision;
    }
}
