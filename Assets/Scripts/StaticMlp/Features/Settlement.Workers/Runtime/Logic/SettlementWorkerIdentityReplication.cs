using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerIdentityReplication
    {
        public const ushort TYPE_ID = 56101;
        public const ReplicationAuthority AUTHORITY = ReplicationAuthority.Server;
        public const ReplicationAudience AUDIENCE = ReplicationAudience.All;
        public const NetDelivery DELIVERY = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in SettlementWorkerIdentity state)
        {
            var writer = BinaryPackWriter.CreateFromPool(8);
            writer.WriteUshort(state.HomeAnchorId);
            writer.WriteUshort(state.RoleId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TYPE_ID, bytes);
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
