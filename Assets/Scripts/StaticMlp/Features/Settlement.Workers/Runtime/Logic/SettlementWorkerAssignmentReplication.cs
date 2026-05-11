using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerAssignmentReplication
    {
        public const ushort TYPE_ID = 56102;
        public const ReplicationAuthority AUTHORITY = ReplicationAuthority.Server;
        public const ReplicationAudience AUDIENCE = ReplicationAudience.All;
        public const NetDelivery DELIVERY = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in SettlementWorkerAssignment state)
        {
            var writer = BinaryPackWriter.CreateFromPool(8);
            writer.WriteByte((byte)state.Status);
            writer.WriteUshort(state.AnchorId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TYPE_ID, bytes);
        }

        public static SettlementWorkerAssignment Read(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new SettlementWorkerAssignment
            {
                Status = (SettlementWorkerAssignmentStatus)reader.ReadByte(),
                AnchorId = reader.ReadUshort()
            };
        }
    }
}
