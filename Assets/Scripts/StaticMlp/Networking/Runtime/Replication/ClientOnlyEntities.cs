using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public static class ClientOnlyEntities
    {
        public static CW.Entity New(ushort clusterId, uint chunkId)
        {
            EnsureStorage(clusterId, chunkId);
            return CW.NewEntityInChunk<Default>(chunkId);
        }

        public static void EnsureStorage(ushort clusterId, uint chunkId)
        {
            if (!CW.ClusterIsRegistered(clusterId))
                CW.RegisterCluster(clusterId);

            if (!CW.ChunkIsRegistered(chunkId))
                CW.RegisterChunk(chunkId, ChunkOwnerType.Self, clusterId);
        }
    }
}
