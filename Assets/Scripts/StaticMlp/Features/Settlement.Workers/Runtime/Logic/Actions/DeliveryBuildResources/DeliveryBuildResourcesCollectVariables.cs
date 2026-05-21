using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Game.Components;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement.Workers
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
            var bestDistanceSq = float.MaxValue;
            var bestSite = default(EntityGID);

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (!SettlementConstructionRules.TryPlanResourceDeposit(
                        in siteState,
                        site,
                        sharedStorageEntity,
                        ConstructionResourcesAccess.GetRemainingResources(site),
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
