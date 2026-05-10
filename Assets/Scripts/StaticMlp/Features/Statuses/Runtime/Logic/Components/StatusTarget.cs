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
    public struct StatusTarget : IComponent, IComponentConfig<StatusTarget>, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public EntityGID Value;

        public ComponentTypeConfig<StatusTarget> Config() =>
            new(guid: new Guid("5f83b1cd-ae0a-4d1a-a6f7-2a7f4a984101"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUlong(Value.Raw);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new StatusTarget
            {
                Value = new EntityGID(reader.ReadUlong()),
            });
        }
    }
}
