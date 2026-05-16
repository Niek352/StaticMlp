using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientEntitySnapshotApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var batch in inbox.EntitySnapshotBatches) {
                for (var i = 0; i < batch.Payloads.Count; i++) {
                    ReplicationRegistry.ApplyServerSnapshot(
                        batch.Payloads[i],
                        FilteredEntitySnapshotLoadMode.PatchExistingOnly);
                }
            }
        }
    }
}
