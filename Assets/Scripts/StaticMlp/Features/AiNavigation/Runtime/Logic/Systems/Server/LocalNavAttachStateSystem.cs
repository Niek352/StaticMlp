using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class LocalNavAttachStateSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<AiNavigationModeState, AiFarSimulationState>>().Entities())
            {
                if (!entity.Has<LocalNavAttachState>())
                {
                    throw new InvalidOperationException(
                        "AiNavigation far-simulation entities must include LocalNavAttachState.");
                }

                entity.Mut<LocalNavAttachState>() = ResolveAttachState(entity.Read<AiFarSimulationState>().LogicalPosition);
            }
        }

        private static LocalNavAttachState ResolveAttachState(float3 logicalPosition)
        {
            var found = false;
            var bestReady = false;
            var bestPriority = int.MinValue;
            var bestDistanceSq = float.MaxValue;
            var bestState = default(LocalNavAttachState);

            foreach (var navAreaEntity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState>>().Entities())
            {
                ref readonly var navArea = ref navAreaEntity.Read<CombatCellNavArea>();
                if (!Contains(in navArea, logicalPosition))
                    continue;

                ref readonly var zoneState = ref navAreaEntity.Read<RuntimeNavMeshZoneState>();
                var isReady = zoneState.BuildState == RuntimeNavMeshBuildState.Ready && zoneState.NavVersion > 0;
                var distanceSq = math.distancesq(navArea.Center, logicalPosition);
                if (!IsBetterCandidate(found, isReady, navArea.Priority, distanceSq, bestReady, bestPriority, bestDistanceSq))
                    continue;

                bestState = new LocalNavAttachState
                {
                    IsReady = isReady,
                    ZoneId = navArea.CellId,
                    NavVersion = zoneState.NavVersion
                };
                bestReady = isReady;
                bestPriority = navArea.Priority;
                bestDistanceSq = distanceSq;
                found = true;
            }

            return bestState;
        }

        private static bool Contains(in CombatCellNavArea navArea, float3 logicalPosition)
        {
            return math.distancesq(navArea.Center, logicalPosition) <= navArea.Radius * navArea.Radius;
        }

        private static bool IsBetterCandidate(
            bool found,
            bool isReady,
            int priority,
            float distanceSq,
            bool bestReady,
            int bestPriority,
            float bestDistanceSq)
        {
            if (!found)
                return true;

            if (isReady != bestReady)
                return isReady;

            if (priority != bestPriority)
                return priority > bestPriority;

            return distanceSq < bestDistanceSq;
        }
    }
}
