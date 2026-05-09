using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<UtpTransportContext>();
            ref var outbox = ref SW.GetResource<NetOutbox>();

            ctx.FlushPendingReliablePackets();
            outbox.FlushComponentBatches();

            foreach (var packet in outbox.Packets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            foreach (var packet in outbox.NetworkEventPackets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            outbox.Clear();
        }
    }
}
