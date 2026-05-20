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

            if (!HasValidAmounts(in request))
                return;

            var storageEntity = SettlementSharedResourcesQuery.GetClientEntity();
            ref readonly var projectedStorage = ref ClientProjection.Read<SettlementSharedResources>(storageEntity);
            ref readonly var projectedState = ref ClientProjection.Read<ConstructionSiteState>(site);
            ref readonly var projectedResources = ref ClientProjection.Read<ConstructionResources>(site);
            if (!SettlementConstructionRules.TryPlanResourceDeposit(
                    in projectedState,
                    in projectedResources,
                    projectedStorage.GetAmount(ResourceCatalog.WoodId),
                    projectedStorage.GetAmount(ResourceCatalog.StoneId),
                    projectedStorage.GetAmount(ResourceCatalog.PlanksId),
                    projectedStorage.GetAmount(ResourceCatalog.SimplePartsId),
                    request.Wood,
                    request.Stone,
                    request.Planks,
                    request.SimpleParts,
                    out var wood,
                    out var stone,
                    out var planks,
                    out var simpleParts))
                return;

            ref var storage = ref ClientProjection.Mut<SettlementSharedResources>(storageEntity);
            storage.Spend(ResourceCatalog.WoodId, wood);
            storage.Spend(ResourceCatalog.StoneId, stone);
            storage.Spend(ResourceCatalog.PlanksId, planks);
            storage.Spend(ResourceCatalog.SimplePartsId, simpleParts);

            ref var state = ref ClientProjection.Mut<ConstructionSiteState>(site);
            ref var resources = ref ClientProjection.Mut<ConstructionResources>(site);
            SettlementConstructionRules.ApplyResourceDeposit(
                ref state,
                ref resources,
                wood,
                stone,
                planks,
                simpleParts);
        }

        public void OnResolved(
            in DepositConstructionResourcesRequestEvent request,
            in DepositConstructionResourcesResultEvent result)
        {
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
