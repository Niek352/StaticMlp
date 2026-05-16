using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientSpawnApplySystem : ISystem {
        private readonly List<EntityGID> _pendingOwnership = new();

        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var spawn in inbox.Spawns) {
                EnsureRemoteChunk(spawn.Gid);
                ReplicationRegistry.ApplyServerSnapshot(
                    spawn.SnapshotPayload,
                    FilteredEntitySnapshotLoadMode.UpsertFromServer);
                PostLoadSpawn(spawn.Gid);
            }

            ApplyPendingOwnership();
        }

        private void PostLoadSpawn(EntityGID gid) {
            if (!gid.TryUnpack<ClientCoreWT>(out var entity))
                throw new Exception($"Spawn snapshot did not create or load entity {gid}.");

            ref readonly var identity = ref entity.Read<NetworkIdentity>();
            Debug.Log($"[ClientSpawnApply] PostLoadSpawn gid={gid.Raw} owner={identity.Owner.Value} localPeerId={NetworkRuntime.LocalPeerId.Value} authority={identity.Authority}");
            entity.Set<NetworkedTag>();
            if (!entity.Has<NetworkReplicationState>())
                entity.Set(new NetworkReplicationState());

            NetArchetypeRegistry.Apply(identity.NetworkArchetypeId, entity);

            if (NetworkRuntime.LocalPeerId.Value == 0) {
                Debug.Log($"[ClientSpawnApply] LocalPeerId is 0, deferring ownership for gid={gid.Raw}");
                _pendingOwnership.Add(gid);
            } else {
                OwnershipTags.ApplyForClient(entity, identity.Owner, identity.Authority);
                Debug.Log($"[ClientSpawnApply] After ApplyForClient: LocalOwned={entity.Has<LocalOwned>()} RemoteOwned={entity.Has<RemoteOwned>()}");
            }

            ReplicationRegistry.InitializeClientCoreInterpolatedState(entity);
        }

        private void ApplyPendingOwnership() {
            if (_pendingOwnership.Count == 0 || NetworkRuntime.LocalPeerId.Value == 0)
                return;

            for (var i = _pendingOwnership.Count - 1; i >= 0; i--) {
                var gid = _pendingOwnership[i];
                if (!gid.TryUnpack<ClientCoreWT>(out var entity)) {
                    _pendingOwnership.RemoveAt(i);
                    continue;
                }

                ref readonly var identity = ref entity.Read<NetworkIdentity>();
                OwnershipTags.ApplyForClient(entity, identity.Owner, identity.Authority);
                Debug.Log($"[ClientSpawnApply] Applied deferred ownership for gid={gid.Raw}: LocalOwned={entity.Has<LocalOwned>()} RemoteOwned={entity.Has<RemoteOwned>()}");
                _pendingOwnership.RemoveAt(i);
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
