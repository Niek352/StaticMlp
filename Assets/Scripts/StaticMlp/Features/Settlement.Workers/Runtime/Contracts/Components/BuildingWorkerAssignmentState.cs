using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "e9cc6b5e-b289-4855-b926-26b14830f97c"
    )]
    public partial struct BuildingWorkerAssignmentState : IComponent, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public SettlementWorkerAssignmentStatus Status;

        [ReplicatedField]
        public ushort AnchorId;

        [ReplicatedField(AllowZeroEntityGid = true)]
        public EntityGID Building;

        [ReplicatedField]
        public byte SlotIndex;

        public SettlementAnchorId Anchor => new(AnchorId);
        public bool IsAssigned => Status == SettlementWorkerAssignmentStatus.Assigned && Building.Raw != 0;
    }
}
