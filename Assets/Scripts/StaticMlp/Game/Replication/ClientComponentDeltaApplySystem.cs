using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientComponentDeltaApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var batch in inbox.ComponentBatches) {
                foreach (var delta in batch.Deltas) {
                    if (!delta.Gid.TryUnpack<ClientCoreWT>(out var e))
                        continue;

                    if (e.Has<LocalOwned>())
                        continue;

                    ReplicationRegistry.ApplyDelta(e, delta);
                }
            }
        }
    }
}
