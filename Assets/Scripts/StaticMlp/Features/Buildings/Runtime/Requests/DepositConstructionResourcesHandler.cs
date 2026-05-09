using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game.Components.Buildings;
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

            if (!ServerPeerPlayers.TryGetPlayer(sourcePeer, out var player)
                || !player.Has<ResourcesInventory>())
                return rejected;

            var inventory = player.Read<ResourcesInventory>();
            var currentState = site.Read<ConstructionSiteState>();
            var currentResources = site.Read<ConstructionResources>();
            if (!ConstructionRules.TryPlanResourceDeposit(
                    in currentState,
                    in currentResources,
                    inventory.Wood,
                    inventory.Stone,
                    request.Wood,
                    request.Stone,
                    out var wood,
                    out var stone))
                return rejected;

            ref var mutableInventory = ref ReplicationMut.Mut<ResourcesInventory>(player);
            var spentWood = mutableInventory.SpendWood(wood);
            var spentStone = mutableInventory.SpendStone(stone);

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
