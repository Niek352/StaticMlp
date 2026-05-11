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
    public struct RaidScheduleState : IComponent, IComponentConfig<RaidScheduleState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField] public ushort RaidIdValue;
        [ReplicatedField] public RaidScheduleStatus Status;
        [ReplicatedField] public uint ActivateAtTick;

        public RaidId RaidId => new(RaidIdValue);

        public ComponentTypeConfig<RaidScheduleState> Config() =>
            new(guid: new Guid("cd4f4133-8327-48f1-a557-1490506cc236"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(RaidIdValue);
            writer.WriteByte((byte)Status);
            writer.WriteUint(ActivateAtTick);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new RaidScheduleState
            {
                RaidIdValue = reader.ReadUshort(),
                Status = (RaidScheduleStatus)reader.ReadByte(),
                ActivateAtTick = reader.ReadUint()
            });
        }
    }
}
