using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Statuses
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 10,
        guid: "0e2fb7fd-8e3c-45fc-a42b-5e197de8d603"
    )]
    public partial struct StatusContext : IComponent, IComponentConfig<StatusContext>, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public EntityGID Source;

        [ReplicatedField]
        public uint RequestId;

        [ReplicatedField]
        public uint RootEffectId;

        [ReplicatedField]
        public byte ChainDepth;

        [ReplicatedField]
        public byte MaxDepth;

        public ComponentTypeConfig<StatusContext> Config() =>
            new(guid: new Guid("0e2fb7fd-8e3c-45fc-a42b-5e197de8d603"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUlong(Source.Raw);
            writer.WriteUint(RequestId);
            writer.WriteUint(RootEffectId);
            writer.WriteByte(ChainDepth);
            writer.WriteByte(MaxDepth);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Source = new EntityGID(reader.ReadUlong());
            RequestId = reader.ReadUint();
            RootEffectId = reader.ReadUint();
            ChainDepth = reader.ReadByte();
            MaxDepth = reader.ReadByte();
        }
    }
}
