using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerRelayClientOwnedStateSystem : ISystem {
        public void Update() {
            ref var outbox = ref SW.GetResource<NetOutbox>();

            foreach (var item in ServerRelayBuffer.Items) {
                foreach (var observer in ServerPeerRegistry.Peers) {
                    if (observer == item.SourcePeer)
                        continue;

                    outbox.EnqueueComponentDelta(observer, item.Delta, NetDelivery.UnreliableSequenced);
                }
            }

            ServerRelayBuffer.Clear();
        }
    }
}
