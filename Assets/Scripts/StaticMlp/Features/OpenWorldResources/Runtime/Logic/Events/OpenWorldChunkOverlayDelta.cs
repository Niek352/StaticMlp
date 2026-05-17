using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldChunkOverlayDelta : IEvent
    {
        public WorldChunkId ChunkId;
        public uint BasisRevision;
        public uint Revision;
        public OpenWorldResourceOverlayDelta[] ResourceDeltas;
    }
}
