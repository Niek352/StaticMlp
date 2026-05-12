using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5
    )]
    public struct SettlementAnchorRef : IComponent, IComponentConfig<SettlementAnchorRef>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort AnchorId;

        public SettlementAnchorId Anchor => new(AnchorId);

        public SettlementAnchorRef(SettlementAnchorId anchor)
        {
            AnchorId = anchor.Value;
        }

        public ComponentTypeConfig<SettlementAnchorRef> Config() =>
            new(guid: new Guid("e271ae45-3257-4897-955f-d411a1dac94f"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(AnchorId);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            self.Set(new SettlementAnchorRef
            {
                AnchorId = reader.ReadUshort()
            });
        }
    }
}
