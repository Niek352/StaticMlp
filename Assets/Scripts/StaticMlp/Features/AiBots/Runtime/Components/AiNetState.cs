using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    /// <summary>
    /// Server-authoritative replicated AI presentation state.
    /// Presence: server world and replicated client AI entities.
    /// </summary>
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.UnreliableSequenced,
        sendRate: 10
    )]
    public struct AiNetState : IComponent, IComponentConfig<AiNetState>, ITrackableAdded, ITrackableChanged,
        ITrackableDeleted
    {
        [ReplicatedField(Compress = true)]
        public AiTaskType CurrentTask;

        [ReplicatedField(Compress = true)]
        public byte LocomotionState;

        [ReplicatedField(Compress = true)]
        public byte CombatState;

        public ComponentTypeConfig<AiNetState> Config() =>
            new(guid: new Guid("f84abac5-9978-4f04-85e7-3f91c43d8192"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort((ushort)CurrentTask);
            writer.WriteByte(LocomotionState);
            writer.WriteByte(CombatState);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new AiNetState
            {
                CurrentTask = (AiTaskType)reader.ReadUshort(),
                LocomotionState = reader.ReadByte(),
                CombatState = reader.ReadByte()
            });
        }
    }
}
