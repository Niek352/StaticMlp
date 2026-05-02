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
            PostLoadCluster(snapshot.ClusterId);
        }

        private static void ApplyChunkSnapshot(ReplicationSnapshotMessage snapshot) {
            if (!CW.ClusterIsRegistered(snapshot.ClusterId))
                CW.RegisterCluster(snapshot.ClusterId);

            if (!CW.ChunkIsRegistered(snapshot.ChunkIdx))
                CW.RegisterChunk(snapshot.ChunkIdx, ChunkOwnerType.Other, snapshot.ClusterId);

            CW.Serializer.LoadChunkSnapshot(snapshot.Payload, gzip: snapshot.Gzip);
            PostLoadCluster(snapshot.ClusterId);
        }

        private static void PostLoadCluster(ushort clusterId) {
            ReadOnlySpan<ushort> clusters = stackalloc ushort[] { clusterId };
            foreach (var e in CW.Query<All<NetworkIdentity>>().Entities(clusters: clusters)) {
                ref readonly var identity = ref e.Read<NetworkIdentity>();
                e.Set<NetworkedTag>();
                PrefabRegistry.Apply(identity.PrefabId, e);
                OwnershipTags.ApplyForClient(e, identity.Owner, identity.Authority);
            }
        }
    }
}
