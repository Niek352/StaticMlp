using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Loadout {
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct PrepareLoadoutCommand : IEvent {
        public const ushort NETWORK_EVENT_ID = 58021;

        public SettlementAnchorId AnchorId;
        public LoadoutModuleId PrimaryModuleId;

        public PrepareLoadoutCommand(SettlementAnchorId anchorId, LoadoutModuleId primaryModuleId) {
            AnchorId = anchorId;
            PrimaryModuleId = primaryModuleId;
        }
    }
}
