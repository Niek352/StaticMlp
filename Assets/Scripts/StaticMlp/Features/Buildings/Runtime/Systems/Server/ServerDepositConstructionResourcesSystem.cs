using FFS.Libraries.StaticEcs;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

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

            var inventory = player.Read<ResourcesInventory>();
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
        }
    }
}
