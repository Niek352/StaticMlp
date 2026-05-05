using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerSteamTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<SteamTransportContext>();
            ref var outbox = ref SW.GetResource<NetOutbox>();

            outbox.FlushComponentBatches();

            foreach (var packet in outbox.Packets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            outbox.Clear();
        }
    }
}
