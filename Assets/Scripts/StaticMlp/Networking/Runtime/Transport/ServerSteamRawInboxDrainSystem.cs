using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerSteamRawInboxDrainSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<SteamTransportContext>();
            ref var inbox = ref SW.GetResource<NetInbox>();
            inbox.Clear();

            while (ctx.RawInbox.Count > 0) {
                var packet = ctx.RawInbox.Dequeue();
                PacketCodec.Decode(packet.SourcePeer, packet.Payload, inbox);
            }
        }
    }
}
