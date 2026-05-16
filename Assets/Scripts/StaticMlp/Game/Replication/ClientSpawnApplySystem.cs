using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientSpawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var spawn in inbox.Spawns) {
                EnsureRemoteChunk(spawn.Gid);
                ReplicationRegistry.ApplyServerSnapshot(
                    spawn.SnapshotPayload,
                    FilteredEntitySnapshotLoadMode.UpsertFromServer);
                PostLoadSpawn(spawn.Gid);
            }
        }

        private static void PostLoadSpawn(EntityGID gid) {
            if (!gid.TryUnpack<ClientCoreWT>(out var entity))
                throw new Exception($"Spawn snapshot did not create or load entity {gid}.");

            ref readonly var identity = ref entity.Read<NetworkIdentity>();
            entity.Set<NetworkedTag>();
            if (!entity.Has<NetworkReplicationState>())
                entity.Set(new NetworkReplicationState());

            NetArchetypeRegistry.Apply(identity.NetworkArchetypeId, entity);
            OwnershipTags.ApplyForClient(entity, identity.Owner, identity.Authority);
            ReplicationRegistry.InitializeClientCoreInterpolatedState(entity);
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
