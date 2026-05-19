using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Loadout {
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct PrepareBossRequestEvent : IEvent {
        public const ushort NETWORK_EVENT_ID = 58022;

        public ushort AnchorIdValue;

        public PrepareBossRequestEvent(SettlementAnchorId anchorId) {
            AnchorIdValue = anchorId.Value;
        }

        public PrepareBossRequestEvent(ushort anchorIdValue) {
            AnchorIdValue = anchorIdValue;
        }

        public SettlementAnchorId AnchorId => new(AnchorIdValue);
    }
}
