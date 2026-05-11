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
    public struct ActiveExpeditionState : IComponent, IComponentConfig<ActiveExpeditionState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField] public ushort ExpeditionIdValue;
        [ReplicatedField] public ExpeditionActivityStatus Status;

        public ExpeditionId ExpeditionId => new(ExpeditionIdValue);

        public ComponentTypeConfig<ActiveExpeditionState> Config() =>
            new(guid: new Guid("755a5754-79b6-42da-a8b3-71788f063f4a"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(ExpeditionIdValue);
            writer.WriteByte((byte)Status);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new ActiveExpeditionState
            {
                ExpeditionIdValue = reader.ReadUshort(),
                Status = (ExpeditionActivityStatus)reader.ReadByte()
            });
        }
    }
}
