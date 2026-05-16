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
        guid: "4b90f4d5-861f-4105-a2a5-48d8dbb5f9c8"
    )]
    public partial struct SettlementCampBuilderJobState : IComponent, IComponentConfig<SettlementCampBuilderJobState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort AnchorId;

        [ReplicatedField(AllowZeroEntityGid = true)]
        public EntityGID AssignedWorker;

        [ReplicatedField(AllowZeroEntityGid = true)]
        public EntityGID TargetSite;

        [ReplicatedField]
        public AiTaskType CurrentTask;

        [ReplicatedField]
        public SettlementWorkerBlockingReason BlockingReason;

        public SettlementAnchorId Anchor => new(AnchorId);

        public ComponentTypeConfig<SettlementCampBuilderJobState> Config() =>
            new(guid: new Guid("4b90f4d5-861f-4105-a2a5-48d8dbb5f9c8"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(AnchorId);
            writer.WriteUlong(AssignedWorker.Raw);
            writer.WriteUlong(TargetSite.Raw);
            writer.WriteUshort((ushort)CurrentTask);
            writer.WriteByte((byte)BlockingReason);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            var anchorId = reader.ReadUshort();
            var assignedWorkerRaw = reader.ReadUlong();
            var targetSiteRaw = reader.ReadUlong();

            AnchorId = anchorId;
            AssignedWorker = assignedWorkerRaw == 0ul ? default : new EntityGID(assignedWorkerRaw);
            TargetSite = targetSiteRaw == 0ul ? default : new EntityGID(targetSiteRaw);
            CurrentTask = (AiTaskType)reader.ReadUshort();
            BlockingReason = (SettlementWorkerBlockingReason)reader.ReadByte();
        }
    }
}
