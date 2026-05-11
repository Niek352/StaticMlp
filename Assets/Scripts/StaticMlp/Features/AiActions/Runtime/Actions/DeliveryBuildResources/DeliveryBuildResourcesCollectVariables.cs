using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Game.Components;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public sealed class DeliveryBuildResourcesCollectVariables : IAiActionVariableCollector
    {
        public const ushort TargetSite = 2201;

        public void Collect(SW.Entity entity)
        {
            var target = FindNearestDepositSite(entity);
            if (target.TryUnpack<ServerWT>(out _))
                AiBlackboardAccess.SetEntity(entity, TargetSite, target);
            else
                AiBlackboardAccess.Remove(entity, TargetSite);
        }

        private static EntityGID FindNearestDepositSite(SW.Entity builder)
        {
            ref readonly var builderState = ref builder.Read<CharacterNetState>();
            var sharedStorageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            ref readonly var sharedResources = ref sharedStorageEntity.Read<SettlementSharedResources>();
            var bestDistanceSq = float.MaxValue;
            var bestSite = default(EntityGID);

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                ref readonly var siteResources = ref site.Read<ConstructionResources>();
                if (!ConstructionRules.TryPlanResourceDeposit(
                        in siteState,
                        in siteResources,
                        sharedResources.GetAmount(ResourceCatalog.WoodId),
                        sharedResources.GetAmount(ResourceCatalog.StoneId),
                        siteResources.RemainingWood,
                        siteResources.RemainingStone,
                        out _,
                        out _))
                    continue;

                ref readonly var transform = ref site.Read<ConstructionTransform>();
                var distanceSq = (transform.Position - builderState.Position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestSite = site.GID;
            }

            return bestSite;
        }
    }
}
