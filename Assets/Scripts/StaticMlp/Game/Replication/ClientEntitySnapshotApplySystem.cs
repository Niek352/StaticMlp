using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientEntitySnapshotApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();
            UnityEngine.Debug.Log($"[ClientEntitySnapshotApply] Batches={inbox.EntitySnapshotBatches.Count}");

            foreach (var batch in inbox.EntitySnapshotBatches) {
                UnityEngine.Debug.Log($"[ClientEntitySnapshotApply] Batch payloads={batch.Payloads.Count}");
                for (var i = 0; i < batch.Payloads.Count; i++) {
                    ReplicationRegistry.ApplyServerSnapshot(
                        batch.Payloads[i],
                        FilteredEntitySnapshotLoadMode.PatchExistingOnly);
                }
            }
        }
    }
}
