using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementCampBuilderJobStateReplication
    {
        public const ushort TYPE_ID = 56103;
        public const ReplicationAuthority AUTHORITY = ReplicationAuthority.Server;
        public const ReplicationAudience AUDIENCE = ReplicationAudience.All;
        public const NetDelivery DELIVERY = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in SettlementCampBuilderJobState state)
        {
            var writer = BinaryPackWriter.CreateFromPool(32);
            writer.WriteUshort(state.AnchorId);
            writer.WriteUlong(state.AssignedWorker.Raw);
            writer.WriteUlong(state.TargetSite.Raw);
            writer.WriteUshort((ushort)state.CurrentTask);
            writer.WriteByte((byte)state.BlockingReason);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TYPE_ID, bytes);
        }

        public static SettlementCampBuilderJobState Read(byte[] payload)
        {
            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            var anchorId = reader.ReadUshort();
            var assignedWorkerRaw = reader.ReadUlong();
            var targetSiteRaw = reader.ReadUlong();
            return new SettlementCampBuilderJobState
            {
                AnchorId = anchorId,
                AssignedWorker = assignedWorkerRaw == 0ul ? default : new EntityGID(assignedWorkerRaw),
                TargetSite = targetSiteRaw == 0ul ? default : new EntityGID(targetSiteRaw),
                CurrentTask = (AiTaskType)reader.ReadUshort(),
                BlockingReason = (SettlementWorkerBlockingReason)reader.ReadByte()
            };
        }
    }
}
