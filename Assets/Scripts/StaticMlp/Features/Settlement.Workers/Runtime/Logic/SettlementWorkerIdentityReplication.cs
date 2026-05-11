using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerIdentityReplication
    {
        public const ushort TypeId = 56101;
        public const ReplicationAuthority Authority = ReplicationAuthority.Server;
        public const ReplicationAudience Audience = ReplicationAudience.All;
        public const NetDelivery Delivery = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in SettlementWorkerIdentity state)
        {
            var writer = BinaryPackWriter.CreateFromPool(8);
            writer.WriteUshort(state.HomeAnchorId);
            writer.WriteUshort(state.RoleId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TypeId, bytes);
        }

        public static SettlementWorkerIdentity Read(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new SettlementWorkerIdentity
            {
                HomeAnchorId = reader.ReadUshort(),
                RoleId = reader.ReadUshort()
            };
        }
    }
}
