using FFS.Libraries.StaticEcs;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game.Components;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public sealed class DepositConstructionResourcesProjector
        : IRequestProjector<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>
    {
        public void Project(in DepositConstructionResourcesRequestEvent request)
        {
            if (!request.Site.TryUnpack<ClientCoreWT>(out var site)
                || !site.Has<ConstructionSiteTag>()
                || !site.Has<Projected<ConstructionSiteState>>()
                || !site.Has<Projected<ConstructionResources>>())
                return;

            if (!TryGetLocalPlayer(out var player))
                return;

            ref readonly var projectedInventory = ref ClientProjection.Read<ResourcesInventory>(player);
            ref readonly var projectedState = ref ClientProjection.Read<ConstructionSiteState>(site);
            ref readonly var projectedResources = ref ClientProjection.Read<ConstructionResources>(site);
            if (!ConstructionRules.TryPlanResourceDeposit(
                    in projectedState,
                    in projectedResources,
                    projectedInventory.Wood,
                    projectedInventory.Stone,
                    request.Wood,
                    request.Stone,
                    out var wood,
                    out var stone))
                return;

            ref var inventory = ref ClientProjection.Mut<ResourcesInventory>(player);
            inventory.SpendWood(wood);
            inventory.SpendStone(stone);

            ref var state = ref ClientProjection.Mut<ConstructionSiteState>(site);
            ref var resources = ref ClientProjection.Mut<ConstructionResources>(site);
            ConstructionRules.ApplyResourceDeposit(
                ref state,
                ref resources,
                wood,
                stone);
        }

        public void OnResolved(
            in DepositConstructionResourcesRequestEvent request,
            in DepositConstructionResourcesResultEvent result)
        {
        }

        private static bool TryGetLocalPlayer(out CW.Entity player)
        {
            foreach (var entity in CW.Query<All<LocalOwned, PlayerTag, ResourcesInventory>>().Entities())
            {
                player = entity;
                return true;
            }

            player = default;
            return false;
        }
    }
}
