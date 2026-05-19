using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.CombatDirector;
using StaticMlp.Features.Frontier;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class AiNavigationDebugSnapshotSystem : ISystem
    {
        public void Update()
        {
            foreach (var navAreaEntity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
                UpdateSnapshot(navAreaEntity);
        }

        private static void UpdateSnapshot(SW.Entity navAreaEntity)
        {
            ref readonly var navArea = ref navAreaEntity.Read<CombatCellNavArea>();
            ref readonly var zoneState = ref navAreaEntity.Read<RuntimeNavMeshZoneState>();
            ref readonly var budget = ref navAreaEntity.Read<CombatCellPerformanceBudget>();
            ref readonly var counters = ref navAreaEntity.Read<NavWorkBudgetCounter>();

            var snapshot = new AiNavigationDebugSnapshot
            {
                CombatCellId = navArea.CellId,
                NavAreaCenter = navArea.Center,
                NavAreaRadius = navArea.Radius,
                NavBuildRadius = navArea.NavBuildRadius,
                SourceCollectRadius = navArea.SourceCollectRadius,
                NavPriority = navArea.Priority,
                NavBuildState = zoneState.BuildState,
                NavVersion = zoneState.NavVersion,
                RequestedNavVersion = zoneState.RequestedNavVersion,
                MaxAliveEnemies = budget.MaxAliveEnemies,
                MaxNewSpawnsPerPeak = budget.MaxNewSpawnsPerPeak,
                MaxPathRequestsPerTick = budget.MaxPathRequestsPerTick,
                MaxReachabilityChecksPerTick = budget.MaxReachabilityChecksPerTick,
                MaxAiDecisionsPerTick = budget.MaxAiDecisionsPerTick,
                MaxNavRebuildStartsPerTick = budget.MaxNavRebuildStartsPerTick,
                PathRequestsThisTick = counters.PathRequestsThisTick,
                ReachabilityChecksThisTick = counters.ReachabilityChecksThisTick,
                RebuildRequestsQueuedThisTick = counters.RebuildRequestsQueuedThisTick,
                RebuildsStartedThisTick = counters.RebuildsStartedThisTick
            };

            PopulateDirectorPhase(ref snapshot);
            PopulateSpawnSourceFacts(in navArea, ref snapshot);
            snapshot.AliveEnemyCount = CountAliveEnemies(in navArea);

            if (navAreaEntity.Has<AiNavigationDebugSnapshot>())
            {
                navAreaEntity.Mut<AiNavigationDebugSnapshot>() = snapshot;
                return;
            }

            navAreaEntity.Set(snapshot);
        }

        private static void PopulateDirectorPhase(ref AiNavigationDebugSnapshot snapshot)
        {
            foreach (var entity in SW.Query<All<FrontierNavWorkPhaseState>>().Entities())
            {
                var phase = entity.Read<FrontierNavWorkPhaseState>().Phase;
                snapshot.HasDirectorPhase = true;
                snapshot.DirectorPhase = phase;
                snapshot.DirectorPhaseId = (byte)phase;
                return;
            }
        }

        private static void PopulateSpawnSourceFacts(in CombatCellNavArea navArea, ref AiNavigationDebugSnapshot snapshot)
        {
            var bestReachableApproxPathCost = float.MaxValue;
            var bestResolvedApproxPathCost = float.MaxValue;

            foreach (var sourceEntity in SW.Query<All<SpawnSource, SpawnSourceNavState>>().Entities())
            {
                ref readonly var spawnSource = ref sourceEntity.Read<SpawnSource>();
                if (!spawnSource.IsActive)
                    continue;

                if (!TryResolveNavArea(spawnSource.Position, out var resolvedNavAreaEntity))
                    continue;

                if (resolvedNavAreaEntity.GID != default && resolvedNavAreaEntity.Read<CombatCellNavArea>().CellId != navArea.CellId)
                    continue;

                ref readonly var navState = ref sourceEntity.Read<SpawnSourceNavState>();
                switch (navState.Status)
                {
                    case SpawnSourceNavStatus.Reachable:
                        snapshot.ReachableSourceCount++;
                        break;
                    case SpawnSourceNavStatus.Unreachable:
                        snapshot.UnreachableSourceCount++;
                        break;
                    default:
                        snapshot.PendingSourceCount++;
                        break;
                }

                if (navState.Status != SpawnSourceNavStatus.Reachable)
                    continue;

                if (navState.ApproxPathCost < bestReachableApproxPathCost)
                {
                    bestReachableApproxPathCost = navState.ApproxPathCost;
                    snapshot.HasBestReachableSource = true;
                    snapshot.BestReachableSource = sourceEntity.GID;
                    snapshot.BestReachableApproxPathCost = navState.ApproxPathCost;
                }

                if (!sourceEntity.Has<ResolvedSpawnPoint>())
                    continue;

                if (navState.ApproxPathCost >= bestResolvedApproxPathCost)
                    continue;

                ref readonly var resolved = ref sourceEntity.Read<ResolvedSpawnPoint>();
                bestResolvedApproxPathCost = navState.ApproxPathCost;
                snapshot.HasResolvedReachableSource = true;
                snapshot.ResolvedReachableSource = resolved.Source;
                snapshot.ResolvedReachablePosition = resolved.Position;
            }
        }

        private static int CountAliveEnemies(in CombatCellNavArea navArea)
        {
            var aliveEnemyCount = 0;

            foreach (var entity in SW.Query<All<MonsterTag, CharacterNetState>, None<IsDiedTag>>().Entities())
            {
                var position = entity.Read<CharacterNetState>().Position;
                if (!Contains(in navArea, position))
                    continue;

                aliveEnemyCount++;
            }

            return aliveEnemyCount;
        }

        private static bool TryResolveNavArea(float3 sourcePosition, out SW.Entity bestEntity)
        {
            bestEntity = default;

            var found = false;
            var bestPriority = int.MinValue;
            var bestDistanceSq = float.MaxValue;

            foreach (var entity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
            {
                ref readonly var navArea = ref entity.Read<CombatCellNavArea>();
                if (!SpawnSourceReachabilityRules.ContainsSource(in navArea, sourcePosition))
                    continue;

                var distanceSq = math.distancesq(navArea.Center, sourcePosition);
                if (!SpawnSourceReachabilityRules.IsBetterAreaCandidate(found, navArea.Priority, distanceSq, bestPriority, bestDistanceSq))
                    continue;

                bestEntity = entity;
                bestPriority = navArea.Priority;
                bestDistanceSq = distanceSq;
                found = true;
            }

            return found;
        }

        private static bool Contains(in CombatCellNavArea navArea, float3 position)
        {
            return math.distancesq(navArea.Center, position) <= navArea.Radius * navArea.Radius;
        }
    }
}
