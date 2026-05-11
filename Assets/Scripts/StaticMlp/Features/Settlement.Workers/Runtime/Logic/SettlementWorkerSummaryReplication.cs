using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerSummaryReplication
    {
        public const ushort TYPE_ID = 56104;
        public const ReplicationAuthority AUTHORITY = ReplicationAuthority.Server;
        public const ReplicationAudience AUDIENCE = ReplicationAudience.All;
        public const NetDelivery DELIVERY = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in SettlementWorkerSummary state)
        {
            var writer = BinaryPackWriter.CreateFromPool(20);
            writer.WriteUshort(state.AnchorId);
            writer.WriteUshort(state.TotalWorkers);
            writer.WriteUshort(state.AssignedWorkers);
            writer.WriteUshort(state.CampBuilderWorkers);
            writer.WriteUshort(state.CampBuilderAssignedWorkers);
            writer.WriteUshort((ushort)state.ActiveTask);
            writer.WriteByte((byte)state.BlockingReason);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TYPE_ID, bytes);
        }

        public static SettlementWorkerSummary Read(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new SettlementWorkerSummary
            {
                AnchorId = reader.ReadUshort(),
                TotalWorkers = reader.ReadUshort(),
                AssignedWorkers = reader.ReadUshort(),
                CampBuilderWorkers = reader.ReadUshort(),
                CampBuilderAssignedWorkers = reader.ReadUshort(),
                ActiveTask = (AiTaskType)reader.ReadUshort(),
                BlockingReason = (SettlementWorkerBlockingReason)reader.ReadByte()
            };
        }
    }
}
