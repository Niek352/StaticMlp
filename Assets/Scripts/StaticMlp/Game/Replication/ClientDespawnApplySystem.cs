using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientDespawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var despawn in inbox.Despawns) {
                if (despawn.Gid.TryUnpack<ClientCoreWT>(out var e)) {
                    if (e.Has<View>()) {
                        ref readonly var view = ref e.Read<View>();
                        view.Value.Unbind();

                        if (view.Value is MonoBehaviour monoBehaviour)
                            Object.Destroy(monoBehaviour.gameObject);
                    }

                    e.Destroy();
                }
            }
        }
    }
}
