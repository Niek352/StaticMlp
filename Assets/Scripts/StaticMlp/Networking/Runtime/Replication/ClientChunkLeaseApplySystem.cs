using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public sealed class ClientChunkLeaseApplySystem : ISystem
    {
        public void Update()
        {
            ref var inbox = ref CW.GetResource<NetInbox>();
            var lease = CW.GetResource<ClientLocalChunkLease>();

            for (var i = 0; i < inbox.ChunkLeases.Count; i++)
                ApplyLease(inbox.ChunkLeases[i], lease);
        }

        private static void ApplyLease(ChunkLeaseMessage message, ClientLocalChunkLease lease)
        {
            lease.Replace(message.ChunkIds);

            for (var i = 0; i < message.ChunkIds.Length; i++)
            {
                var chunkId = message.ChunkIds[i];
                if (!CW.ChunkIsRegistered(chunkId))
                {
                    CW.RegisterChunk(chunkId, ChunkOwnerType.Self, clusterId: 0);
                    continue;
                }

                if (CW.GetChunkClusterId(chunkId) != 0)
                    throw new InvalidOperationException($"Client lease chunk {chunkId} is registered in cluster {CW.GetChunkClusterId(chunkId)} instead of cluster 0.");

                if (CW.GetChunkOwner(chunkId) != ChunkOwnerType.Self)
                    throw new InvalidOperationException($"Client lease chunk {chunkId} is already registered as {CW.GetChunkOwner(chunkId)}.");
            }
        }
    }
}
