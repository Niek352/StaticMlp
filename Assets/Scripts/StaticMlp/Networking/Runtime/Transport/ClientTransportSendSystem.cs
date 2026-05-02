using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<UtpTransportContext>();
            ref var outbox = ref CW.GetResource<NetOutbox>();

            foreach (var packet in outbox.Packets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            outbox.Clear();
        }
    }
}
