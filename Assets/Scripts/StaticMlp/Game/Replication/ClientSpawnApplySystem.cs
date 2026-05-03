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
                    NetworkArchetypeId = spawn.NetworkArchetypeId
                });
                e.Set<NetworkedTag>();

                NetArchetypeRegistry.Apply(spawn.NetworkArchetypeId, e);
                ReplicationRegistry.ApplyInitialState(e, spawn.Components);
                ReplicationRegistry.InitializeClientCoreInterpolatedState(e);
                OwnershipTags.ApplyForClient(e, spawn.Owner, spawn.Authority);
            }
        }
    }
}
