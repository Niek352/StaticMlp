using System;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClaimProductionOutputHandler
        : IRequestHandler<ClaimProductionOutputRequestEvent, ClaimProductionOutputResultEvent>
    {
        private readonly float _interactionRange;

        public ClaimProductionOutputHandler(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public ClaimProductionOutputResultEvent Handle(
            NetworkPeerId sourcePeer,
            in ClaimProductionOutputRequestEvent request)
        {
            var rejected = new ClaimProductionOutputResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Building = request.Building,
                ResourceId = request.ResourceId,
                TransferredAmount = 0
            };

            if (request.RequestedAmount <= 0
                || !request.Building.TryUnpack<ServerWT>(out var building)
                || !building.Has<FinishedBuildingTag>()
                || !building.Has<ConstructionSiteState>()
                || !building.Has<ConstructionTransform>()
                || !building.Has<ProductionStationOperationState>())
            {
                return rejected;
            }

            ref readonly var site = ref building.Read<ConstructionSiteState>();
            if (site.Phase != ConstructionPhase.Completed)
                return rejected;

            var definition = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.OpensQueue) == 0)
                return rejected;

            ref readonly var transform = ref building.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            ref readonly var station = ref building.Read<ProductionStationOperationState>();
            if (!station.Enabled || !ProductionStationRules.IsStationOutputResource(station.Station, request.Resource))
                return rejected;

            var availableOutput = ProductionStationResourceAccess.GetOutput(building, request.Resource);
            if (availableOutput <= 0)
                return rejected;

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            ref readonly var storage = ref storageEntity.Read<SettlementSharedResources>();
            var requested = Math.Min(request.RequestedAmount, availableOutput);
            var accepted = StockpileRules.ClampToCapacity(
                storage.Capacity,
                SettlementSharedResourcesAccess.TotalUsed(storageEntity),
                requested);
            if (accepted <= 0)
                return rejected;

            SW.SendEvent(new ClaimProductionOutputToStorageEvent(
                request.Building,
                request.Resource,
                accepted));

            return new ClaimProductionOutputResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Building = request.Building,
                ResourceId = request.ResourceId,
                TransferredAmount = accepted
            };
        }
    }
}
