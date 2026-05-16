using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public static class ClientOnlyEntities
    {
        public static CW.Entity New()
        {
            var lease = CW.GetResource<ClientLocalChunkLease>();
            for (var i = 0; i < lease.Count; i++)
            {
                if (CW.TryNewEntityInChunk<Default>(out var entity, chunkIdx: lease.GetChunkId(i)))
                    return entity;
            }

            throw new System.InvalidOperationException("Client local chunk lease is missing free entity slots.");
        }
    }
}
