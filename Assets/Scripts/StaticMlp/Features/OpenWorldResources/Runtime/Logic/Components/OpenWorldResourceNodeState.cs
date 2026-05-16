using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "859f7dd8-e326-40be-8ba0-669c9304b3d5"
    )]
    public partial struct OpenWorldResourceNodeState : IComponent, IComponentConfig<OpenWorldResourceNodeState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public long PlacementId;

        [ReplicatedField]
        public ushort KindIdValue;

        [ReplicatedField]
        public int RemainingAmount;

        public ComponentTypeConfig<OpenWorldResourceNodeState> Config() =>
            new(guid: new Guid("859f7dd8-e326-40be-8ba0-669c9304b3d5"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteLong(PlacementId);
            writer.WriteUshort(KindIdValue);
            writer.WriteInt(RemainingAmount);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            PlacementId = reader.ReadLong();
            KindIdValue = reader.ReadUshort();
            RemainingAmount = reader.ReadInt();
        }
    }
}
