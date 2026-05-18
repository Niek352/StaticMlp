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
        guid: "27e11c20-b722-493e-aa56-1727a24a01e3"
    )]
    public partial struct NpcIdentity : IComponent, IComponentConfig<NpcIdentity>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort DefinitionId;

        [ReplicatedField]
        public NpcClass Class;

        [ReplicatedField]
        public NpcAcquisitionPath AcquisitionPath;

        [ReplicatedField]
        public NpcRoleFlags Roles;

        public NpcDefinitionId Definition => new(DefinitionId);

        public ComponentTypeConfig<NpcIdentity> Config() =>
            new(guid: new Guid("27e11c20-b722-493e-aa56-1727a24a01e3"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(DefinitionId);
            writer.WriteByte((byte)Class);
            writer.WriteByte((byte)AcquisitionPath);
            writer.WriteUshort((ushort)Roles);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            DefinitionId = reader.ReadUshort();
            Class = (NpcClass)reader.ReadByte();
            AcquisitionPath = (NpcAcquisitionPath)reader.ReadByte();
            Roles = (NpcRoleFlags)reader.ReadUshort();
        }
    }
}
