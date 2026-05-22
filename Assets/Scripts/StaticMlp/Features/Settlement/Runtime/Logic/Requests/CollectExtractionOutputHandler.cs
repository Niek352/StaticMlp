using System;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class CollectExtractionOutputHandler
        : IRequestHandler<CollectExtractionOutputRequestEvent, CollectExtractionOutputResultEvent>
    {
        private readonly float _interactionRange;

        public CollectExtractionOutputHandler(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public CollectExtractionOutputResultEvent Handle(
            NetworkPeerId sourcePeer,
            in CollectExtractionOutputRequestEvent request)
        {
            var rejected = new CollectExtractionOutputResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Building = request.Building,
                ResourceId = request.ResourceId,
                TransferredAmount = 0
            };

            if (!request.Building.TryUnpack<ServerWT>(out var building)
                || !building.Has<FinishedBuildingTag>()
                || !building.Has<ConstructionSiteState>()
                || !building.Has<ConstructionTransform>()
                || !building.Has<ExtractionOperationState>())
            {
                return rejected;
            }

            ref readonly var site = ref building.Read<ConstructionSiteState>();
            if (site.Phase != ConstructionPhase.Completed)
                return rejected;

            var definition = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ExtractsFromNode) == 0)
                return rejected;

            ref readonly var transform = ref building.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            ref readonly var extraction = ref building.Read<ExtractionOperationState>();
            if (!ExtractionRules.HasOutput(in extraction)
                || request.Resource != extraction.OutputResource
                || request.RequestedAmount <= 0)
            {
                return rejected;
            }

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            ref readonly var storage = ref storageEntity.Read<SettlementSharedResources>();
            var requested = Math.Min(request.RequestedAmount, extraction.OutputBufferAmount);
            var accepted = StockpileRules.ClampToCapacity(
                storage.Capacity,
                SettlementSharedResourcesAccess.TotalUsed(storageEntity),
                requested);
            if (accepted <= 0)
                return rejected;

            SW.SendEvent(new TransferExtractionOutputToStockpileEvent(
                request.Building,
                request.Resource,
                accepted));

            return new CollectExtractionOutputResultEvent
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
