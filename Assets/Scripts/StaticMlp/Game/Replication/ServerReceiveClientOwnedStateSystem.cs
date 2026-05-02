using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerReceiveClientOwnedStateSystem : ISystem {
        public void Update() {
            ref var inbox = ref SW.GetResource<NetInbox>();

            foreach (var batch in inbox.ComponentBatches) {
                foreach (var delta in batch.Deltas) {
                    if (!delta.Gid.TryUnpack<ServerWT>(out var e))
                        continue;

                    if (!e.Has<ClientOwned>() || !e.Has<NetworkIdentity>())
                        continue;

                    ref readonly var net = ref e.Read<NetworkIdentity>();
                    if (net.Owner != batch.SourcePeer)
                        continue;

                    ReplicationRegistry.ApplyDelta(e, delta);
                    ServerRelayBuffer.Add(batch.SourcePeer, delta);
                }
            }
        }
    }
}
