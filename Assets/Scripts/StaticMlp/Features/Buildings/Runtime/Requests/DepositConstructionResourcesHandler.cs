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

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var storage = storageEntity.Read<SettlementSharedResources>();
            var currentState = site.Read<ConstructionSiteState>();
            var currentResources = site.Read<ConstructionResources>();
            if (!ConstructionRules.TryPlanResourceDeposit(
                    in currentState,
                    in currentResources,
                    storage.GetAmount(ResourceCatalog.WoodId),
                    storage.GetAmount(ResourceCatalog.StoneId),
                    request.Wood,
                    request.Stone,
                    out var wood,
                    out var stone))
                return rejected;

            ref var mutableStorage = ref ReplicationMut.Mut<SettlementSharedResources>(storageEntity);
            var spentWood = mutableStorage.Spend(ResourceCatalog.WoodId, wood);
            var spentStone = mutableStorage.Spend(ResourceCatalog.StoneId, stone);

            ref var state = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref var resources = ref ReplicationMut.Mut<ConstructionResources>(site);
            ConstructionRules.ApplyResourceDeposit(
                ref state,
                ref resources,
                spentWood,
                spentStone);

            return new DepositConstructionResourcesResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Site = request.Site,
                AcceptedWood = spentWood,
                AcceptedStone = spentStone
            };
        }
    }
}
