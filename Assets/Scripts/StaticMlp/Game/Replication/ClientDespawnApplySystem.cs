using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientDespawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var despawn in inbox.Despawns) {
                if (despawn.Gid.TryUnpack<ClientCoreWT>(out var e))
                    e.Destroy();
            }
        }
    }
}
