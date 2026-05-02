using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientSpawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var spawn in inbox.Spawns) {
                if (spawn.Gid.TryUnpack<ClientCoreWT>(out _))
                    continue;

                var e = CW.NewEntityByGID<Default>(spawn.Gid);
                e.Set(new NetworkIdentity {
                    Owner = spawn.Owner,
                    Authority = spawn.Authority,
                    PrefabId = spawn.PrefabId
                });
                e.Set<NetworkedTag>();

                PrefabRegistry.Apply(spawn.PrefabId, e);
                ReplicationRegistry.ApplyInitialState(e, spawn.Components);
                OwnershipTags.ApplyForClient(e, spawn.Owner, spawn.Authority);
            }
        }
    }
}
