using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<UtpTransportContext>();
            ref var outbox = ref SW.GetResource<NetOutbox>();

            ctx.FlushPendingReliablePackets();
            outbox.FlushEntitySnapshotBatches();
            outbox.FlushNetworkEventBatches();

            UnityEngine.Debug.Log($"[ServerTransportSend] Packets to send={outbox.Packets.Count}");
            foreach (var packet in outbox.Packets) {
                UnityEngine.Debug.Log($"[ServerTransportSend] Sending to peer={packet.Peer.Value} size={packet.Payload.Length} delivery={packet.Delivery}");
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);
            }

            outbox.Clear();
        }
    }
}
