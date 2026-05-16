using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientSpawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var spawn in inbox.Spawns) {
                EnsureRemoteChunk(spawn.Gid);
                ReplicationRegistry.ApplyServerSnapshot(
                    spawn.SnapshotPayload,
                    FilteredEntitySnapshotLoadMode.UpsertFromServer);
            }
        }

        private static void EnsureRemoteChunk(EntityGID gid) {
            if (!CW.ClusterIsRegistered(gid.ClusterId))
                CW.RegisterCluster(gid.ClusterId);

            if (!CW.ChunkIsRegistered(gid.Chunk)) {
                CW.RegisterChunk(gid.Chunk, ChunkOwnerType.Other, gid.ClusterId);
                return;
            }

            if (CW.GetChunkClusterId(gid.Chunk) != gid.ClusterId && !CW.HasEntitiesInChunk(gid.Chunk))
                CW.ChangeChunkCluster(gid.Chunk, gid.ClusterId);

            if (CW.GetChunkOwner(gid.Chunk) == ChunkOwnerType.Self)
                CW.ChangeChunkOwner(gid.Chunk, ChunkOwnerType.Other);
        }
    }
}
