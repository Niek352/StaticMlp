using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "0d11fbba-0d66-40e4-973b-c19cdfb4ec01"
    )]
    public partial struct ConstructionSiteState : IComponent, IComponentConfig<ConstructionSiteState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField] public ushort BuildingId;
        [ReplicatedField] public ConstructionPhase Phase;

        public ComponentTypeConfig<ConstructionSiteState> Config() =>
            new(guid: new Guid("0d11fbba-0d66-40e4-973b-c19cdfb4ec01"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(BuildingId);
            writer.WriteByte((byte)Phase);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            BuildingId = reader.ReadUshort();
            Phase = (ConstructionPhase)reader.ReadByte();
        }
    }
}
