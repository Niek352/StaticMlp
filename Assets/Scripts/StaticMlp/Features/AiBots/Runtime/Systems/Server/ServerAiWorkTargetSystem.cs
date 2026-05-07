using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Game.Components;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiWorkTargetSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiBlackboard, CharacterNetState>>().Entities())
            {
                ref var blackboard = ref entity.Mut<AiBlackboard>();
                blackboard.Hunger = Saturate(blackboard.Hunger);
                blackboard.Health01 = Saturate(blackboard.Health01);
                blackboard.Fear = Saturate(blackboard.Fear);
                blackboard.WoodStorage01 = Saturate(blackboard.WoodStorage01);

                if (blackboard.EnemyDistance < 0f)
                    blackboard.EnemyDistance = 0f;

                blackboard.WorkTarget = FindNearestBuildableSite(entity);
            }
        }

        private static EntityGID FindNearestBuildableSite(SW.Entity builder)
        {
            ref readonly var builderState = ref builder.Read<CharacterNetState>();
            var bestDistanceSq = float.MaxValue;
            var bestSite = default(EntityGID);

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform, ConstructionProgress>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                ref readonly var resources = ref site.Read<ConstructionResources>();
                if (!ConstructionRules.CanBuild(in siteState, in resources))
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

        private static float Saturate(float value)
        {
            return MathF.Max(0f, MathF.Min(1f, value));
        }
    }
}
