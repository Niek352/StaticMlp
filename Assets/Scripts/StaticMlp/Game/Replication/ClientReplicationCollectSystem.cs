using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientReplicationCollectSystem : ISystem {
        public void Update() {
            ref var outbox = ref CW.GetResource<NetOutbox>();

            foreach (var e in CW.Query<All<LocalOwned, NetworkedTag, NetworkIdentity>>().Entities())
                ReplicationRegistry.CollectDirty(e, outbox, new NetworkPeerId(0));
        }
    }
}
