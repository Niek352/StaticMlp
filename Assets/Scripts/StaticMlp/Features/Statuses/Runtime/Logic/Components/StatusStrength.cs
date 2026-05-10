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
        sendRate: 10
    )]
    public struct StatusStrength : IComponent, IComponentConfig<StatusStrength>, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField(Quantize = 0.01f)]
        public float Power;

        [ReplicatedField]
        public byte Stacks;

        public ComponentTypeConfig<StatusStrength> Config() =>
            new(guid: new Guid("1435a86b-0f3d-4a6f-8868-d4a3d49e6102"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(Power);
            writer.WriteByte(Stacks);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new StatusStrength
            {
                Power = reader.ReadFloat(),
                Stacks = reader.ReadByte(),
            });
        }
    }
}
