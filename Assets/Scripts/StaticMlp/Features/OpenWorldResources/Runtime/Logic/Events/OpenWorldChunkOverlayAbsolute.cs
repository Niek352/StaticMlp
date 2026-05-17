using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldChunkOverlayAbsolute : IEvent
    {
        public WorldChunkId ChunkId;
        public uint Revision;
        public OpenWorldResourceOverlayState[] ResourceStates;
    }
}
