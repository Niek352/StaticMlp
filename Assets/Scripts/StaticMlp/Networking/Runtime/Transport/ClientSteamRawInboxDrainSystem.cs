using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientSteamRawInboxDrainSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<SteamTransportContext>();
            ref var inbox = ref CW.GetResource<NetInbox>();
            inbox.Clear();

            while (ctx.RawInbox.Count > 0) {
                var packet = ctx.RawInbox.Dequeue();
                PacketCodec.Decode(packet.SourcePeer, packet.Payload, inbox);
                var oldPeer = ctx.LocalPeerId;
                ctx.LocalPeerId = NetworkRuntime.LocalPeerId;
                if (oldPeer != ctx.LocalPeerId)
                    SteamTransportContext.Log($"Steam client local peer id set to {ctx.LocalPeerId}");
            }
        }
    }
}
