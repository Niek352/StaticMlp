using System;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class DepositCarriedResourcesToStockpileHandler
        : IRequestHandler<DepositCarriedResourcesToStockpileRequestEvent, DepositCarriedResourcesToStockpileResultEvent>
    {
        private readonly float _interactionRange;

        public DepositCarriedResourcesToStockpileHandler(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public DepositCarriedResourcesToStockpileResultEvent Handle(
            NetworkPeerId sourcePeer,
            in DepositCarriedResourcesToStockpileRequestEvent request)
        {
            var rejected = new DepositCarriedResourcesToStockpileResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Building = request.Building,
                TransferredAmount = 0
            };

            if (!request.Building.TryUnpack<ServerWT>(out var stockpile)
                || !ServerPeerPlayers.TryGetPlayer(sourcePeer, out var player)
                || !player.Has<ResourcesInventory>()
                || !stockpile.Has<FinishedBuildingTag>()
                || !stockpile.Has<ConstructionSiteState>()
                || !stockpile.Has<ConstructionTransform>()
                || !stockpile.Has<StockpileOperationState>())
            {
                return rejected;
            }

            ref readonly var site = ref stockpile.Read<ConstructionSiteState>();
            if (site.Phase != ConstructionPhase.Completed)
                return rejected;

            var definition = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
            if (definition.Id != BuildingCatalogData.StockpileId
                || (definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ProvidesStorage) == 0)
            {
                return rejected;
            }

            ref readonly var transform = ref stockpile.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            var carriedTotal = ResourcesInventoryAccess.TotalAmount(player);
            if (carriedTotal <= 0)
                return rejected;

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            ref readonly var storage = ref storageEntity.Read<SettlementSharedResources>();
            var acceptedBudget = StockpileRules.ClampToCapacity(
                storage.Capacity,
                SettlementSharedResourcesAccess.TotalUsed(storageEntity),
                carriedTotal);
            if (acceptedBudget <= 0)
                return rejected;

            var carriedAmounts = new ResourceAmount[ResourcesInventory.MAX_SLOTS];
            var carriedCount = ResourcesInventoryAccess.CopyAmounts(player, carriedAmounts);
            var transferred = TransferCarriedAmounts(player, storageEntity, carriedAmounts, carriedCount, acceptedBudget);
            if (transferred <= 0)
                return rejected;

            return new DepositCarriedResourcesToStockpileResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Building = request.Building,
                TransferredAmount = transferred
            };
        }

        private static int TransferCarriedAmounts(
            SW.Entity player,
            SW.Entity storageEntity,
            ResourceAmount[] carriedAmounts,
            int carriedCount,
            int acceptedBudget)
        {
            var remainingCapacity = acceptedBudget;
            var transferred = 0;

            for (var i = 0; i < carriedCount && remainingCapacity > 0; i++)
            {
                var carried = carriedAmounts[i];
                ValidateRawResource(carried.Id);

                var requested = Math.Min(carried.Amount, remainingCapacity);
                var spent = ResourcesInventoryAccess.Spend(player, carried.Id, requested);
                if (spent != requested)
                    throw new InvalidOperationException(
                        $"Carried inventory spent {spent} of requested {requested} for resource id {carried.Id.Value}.");

                var accepted = SettlementSharedResourcesAccess.Add(storageEntity, carried.Id, spent);
                if (accepted != spent)
                    throw new InvalidOperationException(
                        $"Settlement storage accepted {accepted} of spent {spent} for resource id {carried.Id.Value}.");

                remainingCapacity -= accepted;
                transferred += accepted;
            }

            return transferred;
        }

        private static void ValidateRawResource(ResourceId resourceId)
        {
            ref readonly var definition = ref ResourceCatalog.Get(resourceId);
            if (definition.Family != ResourceFamily.Raw)
                throw new InvalidOperationException(
                    $"Carried stockpile deposit resource id {resourceId.Value} is {definition.Family}, not raw.");
        }
    }
}
