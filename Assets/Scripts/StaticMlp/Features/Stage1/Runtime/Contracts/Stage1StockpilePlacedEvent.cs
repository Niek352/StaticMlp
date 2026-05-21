using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public readonly struct Stage1StockpilePlacedEvent : IEvent
    {
        private readonly ushort _anchorIdValue;

        public Stage1StockpilePlacedEvent(SettlementAnchorId anchorId)
        {
            _anchorIdValue = anchorId.Value;
        }

        public SettlementAnchorId AnchorId => new(_anchorIdValue);
    }
}
