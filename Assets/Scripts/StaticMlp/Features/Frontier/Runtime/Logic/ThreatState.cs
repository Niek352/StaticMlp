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
        sendRate: 5,
        guid: "86c31ffc-b59f-4cf0-b0ec-f1886e4bcf40"
    )]
    public partial struct ThreatState : IComponent, IComponentConfig<ThreatState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField] public ThreatPhase Phase;
        [ReplicatedField] public ushort ThreatValue;

        public ComponentTypeConfig<ThreatState> Config() =>
            new(guid: new Guid("86c31ffc-b59f-4cf0-b0ec-f1886e4bcf40"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteByte((byte)Phase);
            writer.WriteUshort(ThreatValue);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            Phase = (ThreatPhase)reader.ReadByte();
            ThreatValue = reader.ReadUshort();
        }
    }
}
