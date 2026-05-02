using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;

namespace StaticMlp.Networking.Replication {
    public static class NetworkIdentityReplication {
        public const ushort TypeId = ReplicatedComponentIds.NetworkIdentity;
        public const ReplicationAuthority Authority = ReplicationAuthority.Server;
        public const NetDelivery Delivery = NetDelivery.ReliableSequenced;
        public const ushort SendRate = 0;
        public const byte LayoutVersion = 1;

        public static ComponentDelta CreateDelta(EntityGID gid, in NetworkIdentity identity) {
            var writer = BinaryPackWriter.CreateFromPool(8);
            writer.WriteUshort(identity.Owner.Value);
            writer.WriteByte((byte)identity.Authority);
            writer.WriteUshort(identity.PrefabId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TypeId, bytes);
        }

        public static NetworkIdentity Read(byte[] payload) {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new NetworkIdentity {
                Owner = new NetworkPeerId(reader.ReadUshort()),
                Authority = (NetworkAuthority)reader.ReadByte(),
                PrefabId = reader.ReadUshort()
            };
        }
    }
}
