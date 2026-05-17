using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientSnapshotApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var snapshot in inbox.Snapshots) {
                switch (snapshot.Kind) {
                    case ReplicationSnapshotKind.Cluster:
                        ApplyClusterSnapshot(snapshot);
                        break;
                    case ReplicationSnapshotKind.Chunk:
                        ApplyChunkSnapshot(snapshot);
                        break;
                    case ReplicationSnapshotKind.ClusterEntities:
                        ApplyClusterEntitiesSnapshot(snapshot);
                        break;
                }
            }
        }

        private static void ApplyClusterSnapshot(in ReplicationSnapshotMessage snapshot) {
            if (!CW.ClusterIsRegistered(snapshot.ClusterId))
                CW.RegisterCluster(snapshot.ClusterId);

            RegisterClusterChunks(snapshot);
            DestroyExistingClusterEntities(snapshot);
            CW.Serializer.LoadClusterSnapshot(snapshot.Payload);
            MarkClusterAsRemote(snapshot.ClusterId);
            PostLoadCluster(snapshot.ClusterId);
        }

        private static void ApplyClusterEntitiesSnapshot(in ReplicationSnapshotMessage snapshot) {
            if (!CW.ClusterIsRegistered(snapshot.ClusterId))
                CW.RegisterCluster(snapshot.ClusterId);

            RegisterClusterChunks(snapshot);
            CW.DestroyAllEntitiesInCluster(snapshot.ClusterId);
            ReplicationRegistry.ApplyServerSnapshot(
                snapshot.Payload,
                FilteredEntitySnapshotLoadMode.UpsertFromServer,
                snapshot.Gzip);
            MarkClusterAsRemote(snapshot.ClusterId);
            PostLoadCluster(snapshot.ClusterId);
        }

        private static void ApplyChunkSnapshot(in ReplicationSnapshotMessage snapshot) {
            if (!CW.ClusterIsRegistered(snapshot.ClusterId))
                CW.RegisterCluster(snapshot.ClusterId);

            if (!CW.ChunkIsRegistered(snapshot.ChunkIdx))
                CW.RegisterChunk(snapshot.ChunkIdx, ChunkOwnerType.Other, snapshot.ClusterId);

            CW.Serializer.LoadChunkSnapshot(snapshot.Payload);
            MarkChunkAsRemote(snapshot.ChunkIdx);
            PostLoadCluster(snapshot.ClusterId);
        }

        private static void RegisterClusterChunks(in ReplicationSnapshotMessage snapshot) {
            if (snapshot.ChunkIds == null)
                throw new InvalidOperationException("Cluster snapshot message is missing chunk ids.");

            for (var i = 0; i < snapshot.ChunkIds.Length; i++)
                EnsureRemoteChunk(snapshot.ClusterId, snapshot.ChunkIds[i]);
        }

        private static void EnsureRemoteChunk(ushort clusterId, uint chunkIdx) {
            if (!CW.ChunkIsRegistered(chunkIdx)) {
                CW.RegisterChunk(chunkIdx, ChunkOwnerType.Other, clusterId);
                return;
            }

            var existingClusterId = CW.GetChunkClusterId(chunkIdx);
            if (existingClusterId != clusterId) {
                if (CW.HasEntitiesInChunk(chunkIdx))
                    throw new InvalidOperationException(
                        $"Client chunk {chunkIdx} is already registered in cluster {existingClusterId} with active entities.");

                CW.ChangeChunkCluster(chunkIdx, clusterId);
            }

            if (CW.GetChunkOwner(chunkIdx) == ChunkOwnerType.Self)
                CW.ChangeChunkOwner(chunkIdx, ChunkOwnerType.Other);
        }

        private static void DestroyExistingClusterEntities(in ReplicationSnapshotMessage snapshot) {
            for (var i = 0; i < snapshot.ChunkIds.Length; i++) {
                var chunkIdx = snapshot.ChunkIds[i];
                if (CW.ChunkIsRegistered(chunkIdx) && CW.HasEntitiesInChunk(chunkIdx)) {
                    CW.DestroyAllEntitiesInCluster(snapshot.ClusterId);
                    return;
                }
            }
        }

        private static void MarkClusterAsRemote(ushort clusterId) {
            var chunks = CW.GetClusterChunks(clusterId);
            foreach (var chunkIdx in chunks)
                MarkChunkAsRemote(chunkIdx);
        }

        private static void MarkChunkAsRemote(uint chunkIdx) {
            if (CW.ChunkIsRegistered(chunkIdx) && CW.GetChunkOwner(chunkIdx) == ChunkOwnerType.Self)
                CW.ChangeChunkOwner(chunkIdx, ChunkOwnerType.Other);
        }

        private static void PostLoadCluster(ushort clusterId) {
            ReadOnlySpan<ushort> clusters = stackalloc ushort[] { clusterId };
            foreach (var e in CW.Query<All<NetworkIdentity>>().Entities(clusters: clusters)) {
                ref readonly var identity = ref e.Read<NetworkIdentity>();
                e.Set<NetworkedTag>();
                if (!e.Has<NetworkReplicationState>())
                    e.Set(new NetworkReplicationState());
                NetArchetypeRegistry.Apply(identity.NetworkArchetypeId, e);
                OwnershipTags.ApplyForClient(e, identity.Owner, identity.Authority);
                ReplicationRegistry.InitializeClientCoreInterpolatedState(e);
            }
        }
    }
}
