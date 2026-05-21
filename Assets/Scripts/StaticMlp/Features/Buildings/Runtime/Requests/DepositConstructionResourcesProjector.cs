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
                || !site.Has<Projected<ConstructionResources>>()
                || !site.Has<CW.Multi<ProjectedMulti<ConstructionResourceEntry>>>())
                return;

            if (!HasValidAmounts(request.Resources))
                return;

            var storageEntity = SettlementSharedResourcesQuery.GetClientEntity();
            ref readonly var projectedState = ref ClientProjection.Read<ConstructionSiteState>(site);
            if (!SettlementConstructionRules.TryPlanProjectedResourceDeposit(
                    in projectedState,
                    site,
                    storageEntity,
                    request.Resources,
                    out var acceptedResources))
                return;

            var spentResources = new ResourceAmount[acceptedResources.Length];
            var spentCount = 0;
            for (var i = 0; i < acceptedResources.Length; i++)
            {
                var accepted = acceptedResources[i];
                var spent = SettlementSharedResourcesAccess.SpendProjected(storageEntity, accepted.Id, accepted.Amount);
                if (spent > 0)
                    spentResources[spentCount++] = new ResourceAmount(accepted.Id, spent);
            }

            if (spentCount != spentResources.Length)
            {
                var compact = new ResourceAmount[spentCount];
                System.Array.Copy(spentResources, compact, spentCount);
                spentResources = compact;
            }

            ref var state = ref ClientProjection.Mut<ConstructionSiteState>(site);
            SettlementConstructionRules.ApplyProjectedResourceDeposit(site, ref state, spentResources);
        }

        public void OnResolved(
            in DepositConstructionResourcesRequestEvent request,
            in DepositConstructionResourcesResultEvent result)
        {
        }

        private static bool HasValidAmounts(ResourceAmount[] resources)
        {
            if (resources == null)
                return false;

            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i].Amount < 0)
                    return false;
            }

            return true;
        }
    }
}
