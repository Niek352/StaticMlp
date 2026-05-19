using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.CombatDirector
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "3ee4b71a-e2d1-4446-9301-a36017ab984b"
    )]
    public partial struct DirectorState : IComponent, IComponentConfig<DirectorState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public DirectorPhase Phase;

        [ReplicatedField(Quantize = 0.01f)]
        public float PhaseTimer;

        [ReplicatedField(Quantize = 0.01f)]
        public float TimeSinceLastPeak;

        public ComponentTypeConfig<DirectorState> Config() =>
            new(guid: new Guid("3ee4b71a-e2d1-4446-9301-a36017ab984b"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteByte((byte)Phase);
            writer.WriteFloat(PhaseTimer);
            writer.WriteFloat(TimeSinceLastPeak);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Phase = (DirectorPhase)reader.ReadByte();
            PhaseTimer = reader.ReadFloat();
            TimeSinceLastPeak = reader.ReadFloat();
        }
    }
}
