using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementCampBuilderJobStateReplication
    {
        public const ushort TypeId = 56103;
        public const ReplicationAuthority Authority = ReplicationAuthority.Server;
        public const ReplicationAudience Audience = ReplicationAudience.All;
        public const NetDelivery Delivery = NetDelivery.ReliableSequenced;

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
            return new ComponentDelta(gid, TypeId, bytes);
        }

        public static SettlementCampBuilderJobState Read(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new SettlementCampBuilderJobState
            {
                AnchorId = reader.ReadUshort(),
                AssignedWorker = new EntityGID(reader.ReadUlong()),
                TargetSite = new EntityGID(reader.ReadUlong()),
                CurrentTask = (AiTaskType)reader.ReadUshort(),
                BlockingReason = (SettlementWorkerBlockingReason)reader.ReadByte()
            };
        }
    }
}
