using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerRelayClientOwnedStateSystem : ISystem {
        public void Update() {
            ref var outbox = ref SW.GetResource<NetOutbox>();
            ref var relayBuffer = ref SW.GetResource<ServerRelayBuffer>();
            foreach (var item in relayBuffer.Items) {
                foreach (var observer in ServerPeerRegistry.Peers) {
                    if (observer == item.SourcePeer)
                        continue;

                    outbox.EnqueueEntitySnapshot(observer, item.Payload, item.Delivery);
                }
            }

            relayBuffer.Clear();
        }
    }
}
