using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication {
    public sealed class ClientOwnershipApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var msg in inbox.OwnershipChanges) {
                if (!msg.Gid.TryUnpack<ClientCoreWT>(out var e))
                    continue;

                ref var net = ref e.Mut<NetworkIdentity>();
                net.Owner = msg.NewOwner;
                net.Authority = msg.Authority;
                OwnershipTags.ApplyForClient(e, msg.NewOwner, msg.Authority);
            }
        }
    }
}
