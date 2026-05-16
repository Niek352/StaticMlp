using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<UtpTransportContext>();
            ref var outbox = ref CW.GetResource<NetOutbox>();

            ctx.FlushPendingReliablePackets();
            outbox.FlushEntitySnapshotBatches();
            outbox.FlushNetworkEventBatches();

            UnityEngine.Debug.Log($"[ClientTransportSend] Packets to send={outbox.Packets.Count}");
            foreach (var packet in outbox.Packets) {
                UnityEngine.Debug.Log($"[ClientTransportSend] Sending to peer={packet.Peer.Value} size={packet.Payload.Length} delivery={packet.Delivery}");
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);
            }

            outbox.Clear();
        }
    }
}
