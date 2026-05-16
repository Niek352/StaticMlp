using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "8249f97b-a1dd-4f58-a8e0-f90ac8474588"
    )]
    public partial struct SettlementWorkerAssignment : IComponent, IComponentConfig<SettlementWorkerAssignment>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public SettlementWorkerAssignmentStatus Status;

        [ReplicatedField]
        public ushort AnchorId;

        public SettlementAnchorId Anchor => new(AnchorId);
        public bool IsAssigned => Status == SettlementWorkerAssignmentStatus.Assigned;

        public ComponentTypeConfig<SettlementWorkerAssignment> Config() =>
            new(guid: new Guid("8249f97b-a1dd-4f58-a8e0-f90ac8474588"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteByte((byte)Status);
            writer.WriteUshort(AnchorId);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Status = (SettlementWorkerAssignmentStatus)reader.ReadByte();
            AnchorId = reader.ReadUshort();
        }
    }
}
