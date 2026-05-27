using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public readonly struct ClaimProductionOutputToStorageEvent : IEvent
    {
        public readonly EntityGID ProductionBuilding;
        public readonly ResourceId Resource;
        public readonly int RequestedAmount;

        public ClaimProductionOutputToStorageEvent(
            EntityGID productionBuilding,
            ResourceId resource,
            int requestedAmount)
        {
            ProductionBuilding = productionBuilding;
            Resource = resource;
            RequestedAmount = requestedAmount;
        }
    }
}
