using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public sealed class DepositConstructionResourcesHandler
        : IRequestHandler<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>
    {
        private readonly float _interactionRange;

        public DepositConstructionResourcesHandler(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public DepositConstructionResourcesResultEvent Handle(
            NetworkPeerId sourcePeer,
            in DepositConstructionResourcesRequestEvent request)
        {
            var rejected = new DepositConstructionResourcesResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Site = request.Site
            };

            if (!ConstructionSiteQuery.TryGetConstructionSite(request.Site, out var site))
                return rejected;

            var transform = site.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            if (!HasValidAmounts(in request))
                return rejected;

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var storage = storageEntity.Read<SettlementSharedResources>();
            var currentState = site.Read<ConstructionSiteState>();
            var currentResources = site.Read<ConstructionResources>();
            if (!SettlementConstructionRules.TryPlanResourceDeposit(
                    in currentState,
                    in currentResources,
                    storage.GetAmount(ResourceCatalog.WoodId),
                    storage.GetAmount(ResourceCatalog.StoneId),
                    storage.GetAmount(ResourceCatalog.PlanksId),
                    storage.GetAmount(ResourceCatalog.SimplePartsId),
                    request.Wood,
                    request.Stone,
                    request.Planks,
                    request.SimpleParts,
                    out var wood,
                    out var stone,
                    out var planks,
                    out var simpleParts))
                return rejected;

            ref var mutableStorage = ref ReplicationMut.Mut<SettlementSharedResources>(storageEntity);
            var spentWood = mutableStorage.Spend(ResourceCatalog.WoodId, wood);
            var spentStone = mutableStorage.Spend(ResourceCatalog.StoneId, stone);
            var spentPlanks = mutableStorage.Spend(ResourceCatalog.PlanksId, planks);
            var spentSimpleParts = mutableStorage.Spend(ResourceCatalog.SimplePartsId, simpleParts);

            ref var state = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref var resources = ref ReplicationMut.Mut<ConstructionResources>(site);
            SettlementConstructionRules.ApplyResourceDeposit(
                ref state,
                ref resources,
                spentWood,
                spentStone,
                spentPlanks,
                spentSimpleParts);

            return new DepositConstructionResourcesResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Site = request.Site,
                AcceptedWood = spentWood,
                AcceptedStone = spentStone,
                AcceptedPlanks = spentPlanks,
                AcceptedSimpleParts = spentSimpleParts
            };
        }

        private static bool HasValidAmounts(in DepositConstructionResourcesRequestEvent request)
        {
            return request.Wood >= 0
                   && request.Stone >= 0
                   && request.Planks >= 0
                   && request.SimpleParts >= 0;
        }
    }
}
