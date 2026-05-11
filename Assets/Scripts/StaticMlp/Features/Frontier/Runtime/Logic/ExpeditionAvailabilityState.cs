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
    public struct ExpeditionAvailabilityState : IComponent, IComponentConfig<ExpeditionAvailabilityState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField] public ushort ExpeditionIdValue;
        [ReplicatedField] public ExpeditionAvailabilityStatus Status;

        public ExpeditionId ExpeditionId => new(ExpeditionIdValue);

        public ComponentTypeConfig<ExpeditionAvailabilityState> Config() =>
            new(guid: new Guid("8f1e0f0d-e2d6-456c-ae4a-21e661e9620b"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(ExpeditionIdValue);
            writer.WriteByte((byte)Status);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new ExpeditionAvailabilityState
            {
                ExpeditionIdValue = reader.ReadUshort(),
                Status = (ExpeditionAvailabilityStatus)reader.ReadByte()
            });
        }
    }
}
