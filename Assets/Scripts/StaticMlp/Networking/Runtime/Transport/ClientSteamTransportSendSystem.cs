using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientSteamTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<SteamTransportContext>();
            ref var outbox = ref CW.GetResource<NetOutbox>();

            outbox.FlushComponentBatches();

            foreach (var packet in outbox.Packets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            foreach (var packet in outbox.NetworkEventPackets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            outbox.Clear();
        }
    }
}
