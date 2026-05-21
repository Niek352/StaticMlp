using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Game.Components;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class BuildConstructionCollectVariables : IAiActionVariableCollector
    {
        public const ushort BuildTargetSite = 2101;

        public void Collect(SW.Entity entity)
        {
            var target = FindNearestBuildableSite(entity);
            if (target.TryUnpack<ServerWT>(out _))
                AiBlackboardAccess.SetEntity(entity, BuildTargetSite, target);
            else
                AiBlackboardAccess.Remove(entity, BuildTargetSite);
        }

        private static EntityGID FindNearestBuildableSite(SW.Entity builder)
        {
            ref readonly var builderState = ref builder.Read<CharacterNetState>();
            var bestDistanceSq = float.MaxValue;
            var bestSite = default(EntityGID);

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform, ConstructionProgress>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (!ConstructionRules.CanBuild(site, in siteState))
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
