using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceProxyRef : IComponent
    {
        public long PlacementId;
        public int ChunkX;
        public int ChunkZ;
    }
}
