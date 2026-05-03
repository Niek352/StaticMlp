using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using StaticMlp.Features.ResourcesInventoryMinimal;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerDepositConstructionResourcesSystem : ISystem
    {
        private readonly float _interactionRange;

        public ServerDepositConstructionResourcesSystem(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public void Update()
        {
            NetworkEvents.ForEachServer<DepositConstructionResourcesRequestEvent>(Handle);
        }

        private void Handle(NetworkPeerId sourcePeer, in DepositConstructionResourcesRequestEvent request)
        {
            if (!ConstructionSiteQuery.TryGetServerConstructionSite(request.Site, out var site))
                return;

            if (!NetworkEntityOwnership.IsOwnedBy(site, sourcePeer))
                return;

            var transform = site.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return;

            if (!ServerPeerPlayers.TryGetPlayer(sourcePeer, out var player)
                || !player.Has<ResourcesInventory>())
                return;

            ref var inventory = ref player.Mut<ResourcesInventory>();
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
                return;

            var spentWood = inventory.SpendWood(wood);
            var spentStone = inventory.SpendStone(stone);

            ref var state = ref site.Mut<ConstructionSiteState>();
            ref var resources = ref site.Mut<ConstructionResources>();
            ConstructionRules.ApplyResourceDeposit(
                ref state,
                ref resources,
                spentWood,
                spentStone);
        }
    }
}
