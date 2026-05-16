using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerReceiveClientOwnedStateSystem : ISystem {
        public void Update() {
            ref var inbox = ref SW.GetResource<NetInbox>();

            foreach (var batch in inbox.EntitySnapshotBatches) {
                for (var i = 0; i < batch.Payloads.Count; i++)
                    ReplicationRegistry.ApplyClientOwnerSnapshot(batch.Payloads[i], batch.SourcePeer);
            }
        }
    }
}
