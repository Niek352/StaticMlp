using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientSpawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var spawn in inbox.Spawns) {
                if (spawn.Gid.TryUnpack<ClientCoreWT>(out _))
                    continue;

                EnsureRemoteChunk(spawn.Gid);
                var e = CW.NewEntityByGID(spawn.EntityType, spawn.Gid);
                e.Set(new NetworkIdentity {
                    Owner = spawn.Owner,
                    Authority = spawn.Authority,
                    NetworkArchetypeId = spawn.NetworkArchetypeId
                });
                e.Set<NetworkedTag>();
                e.Set(new NetworkReplicationState());

                NetArchetypeRegistry.Apply(spawn.NetworkArchetypeId, e);
                ReplicationRegistry.ApplyInitialState(e, spawn.Components);
                ReplicationRegistry.InitializeClientCoreInterpolatedState(e);
                OwnershipTags.ApplyForClient(e, spawn.Owner, spawn.Authority);
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
