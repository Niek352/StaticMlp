using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5
    )]
    public struct BossEncounterState : IComponent, IComponentConfig<BossEncounterState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField] public ushort BossIdValue;
        [ReplicatedField] public BossEncounterStatus Status;

        public BossId BossId => new(BossIdValue);

        public ComponentTypeConfig<BossEncounterState> Config() =>
            new(guid: new Guid("31db70ec-ecdd-4c31-89a6-c85c3507cfe2"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(BossIdValue);
            writer.WriteByte((byte)Status);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new BossEncounterState
            {
                BossIdValue = reader.ReadUshort(),
                Status = (BossEncounterStatus)reader.ReadByte()
            });
        }
    }
}
