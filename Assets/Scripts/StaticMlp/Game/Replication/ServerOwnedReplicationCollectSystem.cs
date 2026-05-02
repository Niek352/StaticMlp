using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerOwnedReplicationCollectSystem : ISystem {
        public void Update() {
            ref var outbox = ref SW.GetResource<NetOutbox>();

            foreach (var e in SW.Query<All<ServerOwned, NetworkedTag, NetworkIdentity>>().Entities()) {
                foreach (var peer in ServerPeerRegistry.Peers)
                    ReplicationRegistry.CollectDirty(e, outbox, peer);
            }
        }
    }
}
