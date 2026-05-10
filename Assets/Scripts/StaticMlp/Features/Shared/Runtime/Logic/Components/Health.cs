using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Shared
{
    /// <summary>
    /// Server-authoritative replicated combat state.
    /// Presence: server world and replicated client actor entities.
    /// </summary>
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 10
    )]
    public struct Health : IComponent, IComponentConfig<Health>, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField(Quantize = 0.01f)]
        public float Current;

        [ReplicatedField(Quantize = 0.01f)]
        public float Max;

        public ComponentTypeConfig<Health> Config() =>
            new(guid: new Guid("0a82e952-f0ee-4dca-9f7d-0eb3217b6a31"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(Current);
            writer.WriteFloat(Max);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new Health
            {
                Current = reader.ReadFloat(),
                Max = reader.ReadFloat(),
            });
        }
    }
}
