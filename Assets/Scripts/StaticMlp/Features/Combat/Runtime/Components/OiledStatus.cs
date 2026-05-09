using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    /// <summary>
    /// Server-authoritative replicated gameplay status.
    /// Presence: server world and replicated client combat actors while the effect is active.
    /// </summary>
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 10
    )]
    public struct OiledStatus : IComponent, IComponentConfig<OiledStatus>, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField(Quantize = 0.01f)]
        public float RemainingTime;

        [ReplicatedField(Quantize = 0.01f)]
        public float Power;

        [ReplicatedField]
        public byte Stacks;

        [ReplicatedField]
        public ulong SourceRaw;

        [ReplicatedField]
        public uint RequestId;

        [ReplicatedField]
        public uint RootEffectId;

        [ReplicatedField]
        public byte ChainDepth;

        [ReplicatedField]
        public byte MaxDepth;

        public ComponentTypeConfig<OiledStatus> Config() =>
            new(guid: new Guid("12cc7e66-6608-4daf-9af8-db2707060807"));

        public EntityGID Source
        {
            readonly get => new(SourceRaw);
            set => SourceRaw = value.Raw;
        }

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteFloat(RemainingTime);
            writer.WriteFloat(Power);
            writer.WriteByte(Stacks);
            writer.WriteUlong(SourceRaw);
            writer.WriteUint(RequestId);
            writer.WriteUint(RootEffectId);
            writer.WriteByte(ChainDepth);
            writer.WriteByte(MaxDepth);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new OiledStatus
            {
                RemainingTime = reader.ReadFloat(),
                Power = reader.ReadFloat(),
                Stacks = reader.ReadByte(),
                SourceRaw = reader.ReadUlong(),
                RequestId = reader.ReadUint(),
                RootEffectId = reader.ReadUint(),
                ChainDepth = reader.ReadByte(),
                MaxDepth = reader.ReadByte(),
            });
        }
    }
}
