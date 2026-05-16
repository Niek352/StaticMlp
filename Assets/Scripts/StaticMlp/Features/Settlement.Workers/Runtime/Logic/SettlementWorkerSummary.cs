using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "3cc73376-44db-4a4d-8fe3-c42ef153f7c8"
    )]
    public partial struct SettlementWorkerSummary : IComponent, IComponentConfig<SettlementWorkerSummary>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort AnchorId;

        [ReplicatedField]
        public ushort TotalWorkers;

        [ReplicatedField]
        public ushort AssignedWorkers;

        [ReplicatedField]
        public ushort CampBuilderWorkers;

        [ReplicatedField]
        public ushort CampBuilderAssignedWorkers;

        [ReplicatedField]
        public AiTaskType ActiveTask;

        [ReplicatedField]
        public SettlementWorkerBlockingReason BlockingReason;

        public SettlementAnchorId Anchor => new(AnchorId);

        public ComponentTypeConfig<SettlementWorkerSummary> Config() =>
            new(guid: new Guid("3cc73376-44db-4a4d-8fe3-c42ef153f7c8"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(AnchorId);
            writer.WriteUshort(TotalWorkers);
            writer.WriteUshort(AssignedWorkers);
            writer.WriteUshort(CampBuilderWorkers);
            writer.WriteUshort(CampBuilderAssignedWorkers);
            writer.WriteUshort((ushort)ActiveTask);
            writer.WriteByte((byte)BlockingReason);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            AnchorId = reader.ReadUshort();
            TotalWorkers = reader.ReadUshort();
            AssignedWorkers = reader.ReadUshort();
            CampBuilderWorkers = reader.ReadUshort();
            CampBuilderAssignedWorkers = reader.ReadUshort();
            ActiveTask = (AiTaskType)reader.ReadUshort();
            BlockingReason = (SettlementWorkerBlockingReason)reader.ReadByte();
        }
    }
}
