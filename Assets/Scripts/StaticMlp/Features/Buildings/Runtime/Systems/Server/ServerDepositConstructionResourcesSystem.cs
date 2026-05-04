using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Features.ResourcesInventoryMinimal;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerDepositConstructionResourcesSystem : ISystem
    {
        private readonly float _interactionRange;
        private EventReceiver<ServerWT, NetworkEventFromClient<DepositConstructionResourcesRequestEvent>> _requests;

        public ServerDepositConstructionResourcesSystem(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<DepositConstructionResourcesRequestEvent>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
            {
                var request = evt.Value;
                Handle(in request);
            }
        }

        private void Handle(in NetworkEventFromClient<DepositConstructionResourcesRequestEvent> request)
        {
            var sourcePeer = request.SourcePeer;
            if (!ConstructionSiteQuery.TryGetConstructionSite(request.Value.Site, out var site))
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
                    request.Value.Wood,
                    request.Value.Stone,
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
