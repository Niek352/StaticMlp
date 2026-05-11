using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerAssignmentReplication
    {
        public const ushort TypeId = 56102;
        public const ReplicationAuthority Authority = ReplicationAuthority.Server;
        public const ReplicationAudience Audience = ReplicationAudience.All;
        public const NetDelivery Delivery = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in SettlementWorkerAssignment state)
        {
            var writer = BinaryPackWriter.CreateFromPool(8);
            writer.WriteByte((byte)state.Status);
            writer.WriteUshort(state.AnchorId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TypeId, bytes);
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
