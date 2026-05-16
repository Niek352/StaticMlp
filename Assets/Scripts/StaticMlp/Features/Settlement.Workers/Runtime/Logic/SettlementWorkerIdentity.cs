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
        guid: "83d6a5f5-44e1-40df-b527-9b2d3a1c2fd4"
    )]
    public partial struct SettlementWorkerIdentity : IComponent, IComponentConfig<SettlementWorkerIdentity>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort HomeAnchorId;

        [ReplicatedField]
        public ushort RoleId;

        public SettlementAnchorId HomeAnchor => new(HomeAnchorId);
        public WorkerRoleId Role => new(RoleId);

        public ComponentTypeConfig<SettlementWorkerIdentity> Config() =>
            new(guid: new Guid("83d6a5f5-44e1-40df-b527-9b2d3a1c2fd4"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(HomeAnchorId);
            writer.WriteUshort(RoleId);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            HomeAnchorId = reader.ReadUshort();
            RoleId = reader.ReadUshort();
        }
    }
}
