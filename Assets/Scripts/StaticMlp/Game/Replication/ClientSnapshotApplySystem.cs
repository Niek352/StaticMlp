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
                }
            }
        }

        private static void ApplyClusterSnapshot(ReplicationSnapshotMessage snapshot) {
            if (!CW.ClusterIsRegistered(snapshot.ClusterId))
                CW.RegisterCluster(snapshot.ClusterId);

            CW.Serializer.LoadClusterSnapshot(snapshot.Payload, gzip: snapshot.Gzip);
            MarkClusterAsRemote(snapshot.ClusterId);
            PostLoadCluster(snapshot.ClusterId);
        }

        private static void ApplyChunkSnapshot(ReplicationSnapshotMessage snapshot) {
            if (!CW.ClusterIsRegistered(snapshot.ClusterId))
                CW.RegisterCluster(snapshot.ClusterId);

            if (!CW.ChunkIsRegistered(snapshot.ChunkIdx))
                CW.RegisterChunk(snapshot.ChunkIdx, ChunkOwnerType.Other, snapshot.ClusterId);

            CW.Serializer.LoadChunkSnapshot(snapshot.Payload, gzip: snapshot.Gzip);
            MarkChunkAsRemote(snapshot.ChunkIdx);
            PostLoadCluster(snapshot.ClusterId);
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
                NetArchetypeRegistry.Apply(identity.NetworkArchetypeId, e);
                OwnershipTags.ApplyForClient(e, identity.Owner, identity.Authority);
                ReplicationRegistry.InitializeClientCoreInterpolatedState(e);
            }
        }
    }
}
