using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Npc
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "b7e9c3a1-5d2f-4e8b-9c1d-3a8f5e2b7c4d"
    )]
    public partial struct NpcRosterRecord : IComponent, IComponentConfig<NpcRosterRecord>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort DefinitionId;

        [ReplicatedField]
        public NpcClass Class;

        [ReplicatedField]
        public NpcAcquisitionPath AcquisitionPath;

        [ReplicatedField]
        public NpcRosterState State;

        [ReplicatedField]
        public uint CreatedServerTick;

        public NpcDefinitionId Definition => new(DefinitionId);

        public ComponentTypeConfig<NpcRosterRecord> Config() =>
            new(guid: new Guid("b7e9c3a1-5d2f-4e8b-9c1d-3a8f5e2b7c4d"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(DefinitionId);
            writer.WriteByte((byte)Class);
            writer.WriteByte((byte)AcquisitionPath);
            writer.WriteByte((byte)State);
            writer.WriteUint(CreatedServerTick);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            DefinitionId = reader.ReadUshort();
            Class = (NpcClass)reader.ReadByte();
            AcquisitionPath = (NpcAcquisitionPath)reader.ReadByte();
            State = (NpcRosterState)reader.ReadByte();
            CreatedServerTick = reader.ReadUint();
        }
    }
}
