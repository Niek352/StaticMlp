using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerReceiveClientOwnedStateSystem : ISystem {
        public void Update() {
            ref var inbox = ref SW.GetResource<NetInbox>();
            ref var relayBuffer = ref SW.GetResource<ServerRelayBuffer>();
            Debug.Log($"[ServerReceiveClientOwned] EntitySnapshotBatches count={inbox.EntitySnapshotBatches.Count}");
            foreach (var batch in inbox.EntitySnapshotBatches) {
                Debug.Log($"[ServerReceiveClientOwned] Batch from peer={batch.SourcePeer.Value}, payloads={batch.Payloads.Count}");
                for (var i = 0; i < batch.Payloads.Count; i++)
                    ReplicationRegistry.ApplyClientOwnerSnapshot(batch.Payloads[i], batch.SourcePeer, relayBuffer);
            }
        }
    }
}
