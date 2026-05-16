using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Transport;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerOwnedReplicationCollectSystem : ISystem {
        public void Update() {
            ref var outbox = ref SW.GetResource<NetOutbox>();
            var dirtyCount = SW.Query<All<NetworkedTag, NetworkDirty>>().EntitiesCount();
            Debug.Log($"[ServerOwnedReplicationCollect] Networked+Dirty count={dirtyCount}, peers={ServerPeerRegistry.Peers.Count}");
            ReplicationRegistry.CollectServerAuthorityDirty(outbox, ServerPeerRegistry.Peers);
        }
    }
}
