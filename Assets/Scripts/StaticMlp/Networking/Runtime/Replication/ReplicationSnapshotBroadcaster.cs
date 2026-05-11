using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public static class ReplicationSnapshotBroadcaster {
        public static void SendClusterSnapshot(NetworkPeerId peer, ushort clusterId, bool gzip = true) {
            ref var outbox = ref SW.GetResource<NetOutbox>();
            outbox.Enqueue(peer, PacketCodec.EncodeSnapshot(new ReplicationSnapshotMessage {
                Kind = ReplicationSnapshotKind.Cluster,
                ClusterId = clusterId,
                ChunkIdx = 0,
                Gzip = gzip,
                Payload = SW.Serializer.CreateClusterSnapshot(
                    clusterId,
                    withCustomSnapshotData: false,
                    gzip: gzip,
                    strategy: ChunkWritingStrategy.All,
                    withEntitiesData: true
                )
            }), NetDelivery.ReliableSequenced);
        }

        public static void SendChunkSnapshot(NetworkPeerId peer, uint chunkIdx, bool gzip = true) {
            var clusterId = SW.GetChunkClusterId(chunkIdx);
            ref var outbox = ref SW.GetResource<NetOutbox>();
            outbox.Enqueue(peer, PacketCodec.EncodeSnapshot(new ReplicationSnapshotMessage {
                Kind = ReplicationSnapshotKind.Chunk,
                ClusterId = clusterId,
                ChunkIdx = chunkIdx,
                Gzip = gzip,
                Payload = SW.Serializer.CreateChunkSnapshot(
                    chunkIdx,
                    withCustomSnapshotData: false,
                    gzip: gzip,
                    withEntitiesData: true
                )
            }), NetDelivery.ReliableSequenced);
        }
    }
}
