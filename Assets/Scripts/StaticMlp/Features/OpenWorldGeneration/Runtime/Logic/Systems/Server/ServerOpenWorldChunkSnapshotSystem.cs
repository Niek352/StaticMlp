using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ServerOpenWorldChunkSnapshotSystem : ISystem
    {
        private readonly Dictionary<ushort, byte[]> _snapshotPacketsByCluster = new();

        public void Update()
        {
            var state = SW.GetResource<OpenWorldChunkStreamingState>();
            var runtime = SW.GetResource<OpenWorldGenerationServerRuntime>();
            var pending = state.PendingSnapshots;
            var clusterBuilds = 0;
            var writeIndex = 0;

            for (var readIndex = 0; readIndex < pending.Count; readIndex++)
            {
                var request = pending[readIndex];
                if (!TryGetSnapshotPacket(request.ClusterId, runtime.MaxClusterSnapshotsPerFrame, ref clusterBuilds, out var packet))
                {
                    pending[writeIndex++] = request;
                    continue;
                }

                ReplicationSnapshotBroadcaster.SendPrebuiltSnapshotPacket(request.Peer, packet);
            }

            if (writeIndex == 0)
                pending.Clear();
            else if (writeIndex < pending.Count)
                pending.RemoveRange(writeIndex, pending.Count - writeIndex);

            _snapshotPacketsByCluster.Clear();
        }

        private bool TryGetSnapshotPacket(
            ushort clusterId,
            int maxClusterBuilds,
            ref int clusterBuilds,
            out byte[] packet)
        {
            if (_snapshotPacketsByCluster.TryGetValue(clusterId, out packet))
                return true;

            if (clusterBuilds >= maxClusterBuilds)
                return false;

            packet = CreateClusterEntitiesSnapshotPacket(clusterId);
            _snapshotPacketsByCluster.Add(clusterId, packet);
            clusterBuilds++;
            return true;
        }
        
        public static byte[] CreateClusterEntitiesSnapshotPacket(ushort clusterId, bool gzip = true) {
            return PacketCodec.EncodeSnapshot(new ReplicationSnapshotMessage {
                Kind = ReplicationSnapshotKind.ClusterEntities,
                ClusterId = clusterId,
                ChunkIdx = 0,
                ChunkIds = ReplicationSnapshotBroadcaster.CopyClusterChunks(clusterId),
                Gzip = gzip,
                Payload = ReplicationRegistry.CreatePublicClusterStateSnapshot(clusterId, gzip)
            });
        }
    }
}
