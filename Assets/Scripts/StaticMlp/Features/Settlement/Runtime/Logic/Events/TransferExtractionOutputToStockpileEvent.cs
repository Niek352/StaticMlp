using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public readonly struct TransferExtractionOutputToStockpileEvent : IEvent
    {
        public readonly EntityGID ExtractionBuilding;
        public readonly ResourceId Resource;
        public readonly int RequestedAmount;

        public TransferExtractionOutputToStockpileEvent(
            EntityGID extractionBuilding,
            ResourceId resource,
            int requestedAmount)
        {
            ExtractionBuilding = extractionBuilding;
            Resource = resource;
            RequestedAmount = requestedAmount;
        }
    }
}
