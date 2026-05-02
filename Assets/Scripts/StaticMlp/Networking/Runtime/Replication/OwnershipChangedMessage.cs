using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public readonly struct OwnershipChangedMessage {
        public readonly EntityGID Gid;
        public readonly NetworkPeerId NewOwner;
        public readonly NetworkAuthority Authority;

        public OwnershipChangedMessage(EntityGID gid, NetworkPeerId newOwner, NetworkAuthority authority) {
            Gid = gid;
            NewOwner = newOwner;
            Authority = authority;
        }
    }
}
