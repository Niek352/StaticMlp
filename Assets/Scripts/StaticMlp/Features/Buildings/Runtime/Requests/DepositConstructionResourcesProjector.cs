using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
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

            var storageEntity = SettlementSharedResourcesQuery.GetClientEntity();
            ref readonly var projectedStorage = ref ClientProjection.Read<SettlementSharedResources>(storageEntity);
            ref readonly var projectedState = ref ClientProjection.Read<ConstructionSiteState>(site);
            ref readonly var projectedResources = ref ClientProjection.Read<ConstructionResources>(site);
            if (!ConstructionRules.TryPlanResourceDeposit(
                    in projectedState,
                    in projectedResources,
                    projectedStorage.GetAmount(ResourceCatalog.WoodId),
                    projectedStorage.GetAmount(ResourceCatalog.StoneId),
                    request.Wood,
                    request.Stone,
                    out var wood,
                    out var stone))
                return;

            ref var storage = ref ClientProjection.Mut<SettlementSharedResources>(storageEntity);
            storage.Spend(ResourceCatalog.WoodId, wood);
            storage.Spend(ResourceCatalog.StoneId, stone);

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
    }
}
