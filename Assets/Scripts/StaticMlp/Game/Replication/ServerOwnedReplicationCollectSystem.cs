using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerOwnedReplicationCollectSystem : ISystem {
        public void Update() {
            ref var outbox = ref SW.GetResource<NetOutbox>();

            ReplicationRegistry.CollectServerAuthorityDirty(outbox, ServerPeerRegistry.Peers);
        }
    }
}
