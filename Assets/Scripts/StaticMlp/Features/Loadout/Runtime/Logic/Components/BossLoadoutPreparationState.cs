using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Loadout
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "4a6ac13f-d0a4-4180-b15a-c544f665fafd"
    )]
    public partial struct BossLoadoutPreparationState : IComponent, IComponentConfig<BossLoadoutPreparationState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public BossLoadoutPreparationStatus Status;

        public ComponentTypeConfig<BossLoadoutPreparationState> Config() =>
            new(guid: new Guid("4a6ac13f-d0a4-4180-b15a-c544f665fafd"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteByte((byte)Status);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Status = (BossLoadoutPreparationStatus)reader.ReadByte();
        }
    }
}
