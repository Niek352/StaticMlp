using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Features.ResourcesInventoryMinimal;
using UnityEngine;

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
            ref var inbox = ref SW.GetResource<NetInbox>();

            foreach (var evt in inbox.Events)
            {
                if (evt.EventTypeId != GameplayEventTypeIds.DepositConstructionResourcesRequest)
                    continue;

                if (!ConstructionEventCodec.TryReadDeposit(evt.Payload, out var request))
                    continue;

                if (!TryGetSite(request.Site, out var site))
                    continue;

                if (!ServerConstructionAuthorization.OwnsSite(site, evt.SourcePeer))
                    continue;

                var transform = site.Read<ConstructionTransform>();
                if (!ServerConstructionAuthorization.IsPlayerNear(evt.SourcePeer, transform.Position, _interactionRange))
                    continue;

                if (!ServerConstructionAuthorization.TryGetPlayer(evt.SourcePeer, out var player)
                    || !player.Has<ResourcesInventory>())
                    continue;

                ref var state = ref site.Mut<ConstructionSiteState>();
                if (state.Phase != ConstructionPhase.WaitingForResources && state.Phase != ConstructionPhase.ReadyToBuild)
                    continue;

                ref var resources = ref site.Mut<ConstructionResources>();
                ref var inventory = ref player.Mut<ResourcesInventory>();

                var requestedWood = Mathf.Min(Mathf.Max(0, request.Wood), resources.RemainingWood);
                var requestedStone = Mathf.Min(Mathf.Max(0, request.Stone), resources.RemainingStone);
                resources.WoodDelivered += inventory.SpendWood(requestedWood);
                resources.StoneDelivered += inventory.SpendStone(requestedStone);

                if (resources.IsComplete)
                    state.Phase = ConstructionPhase.ReadyToBuild;
            }
        }

        private static bool TryGetSite(EntityGID gid, out SW.Entity site)
        {
            if (gid.TryUnpack<ServerWT>(out site)
                && site.Has<ConstructionSiteTag>()
                && site.Has<ConstructionSiteState>()
                && site.Has<ConstructionResources>()
                && site.Has<ConstructionTransform>())
                return true;

            site = default;
            return false;
        }
    }
}
