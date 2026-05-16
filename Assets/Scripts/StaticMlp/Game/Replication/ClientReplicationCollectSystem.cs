using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Networking.Replication
{
    public sealed class ClientReplicationCollectSystem : ISystem
    {
        public void Update()
        {
            ref var outbox = ref CW.GetResource<NetOutbox>();
            var count = CW.Query<All<LocalOwned, NetworkDirty>>().EntitiesCount();
            Debug.Log($"[ClientReplicationCollect] LocalOwned+Dirty count={count}");
            ReplicationRegistry.CollectClientOwnedDirty(outbox, new NetworkPeerId(0));
        }
    }
}