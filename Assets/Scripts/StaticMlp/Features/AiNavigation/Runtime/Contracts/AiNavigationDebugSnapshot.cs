using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public struct AiNavigationDebugSnapshot : IComponent
    {
        public int CombatCellId;
        public float3 NavAreaCenter;
        public float NavAreaRadius;
        public float NavBuildRadius;
        public float SourceCollectRadius;
        public int NavPriority;
        public RuntimeNavMeshBuildState NavBuildState;
        public int NavVersion;
        public int RequestedNavVersion;
        public bool HasDirectorPhase;
        public FrontierNavWorkPhase DirectorPhase;
        public byte DirectorPhaseId;
        public int ReachableSourceCount;
        public int UnreachableSourceCount;
        public int PendingSourceCount;
        public bool HasBestReachableSource;
        public EntityGID BestReachableSource;
        public float BestReachableApproxPathCost;
        public bool HasResolvedReachableSource;
        public EntityGID ResolvedReachableSource;
        public float3 ResolvedReachablePosition;
        public int MaxAliveEnemies;
        public int AliveEnemyCount;
        public int MaxNewSpawnsPerPeak;
        public int MaxPathRequestsPerTick;
        public int MaxReachabilityChecksPerTick;
        public int MaxAiDecisionsPerTick;
        public int MaxNavRebuildStartsPerTick;
        public int PathRequestsThisTick;
        public int ReachabilityChecksThisTick;
        public int RebuildRequestsQueuedThisTick;
        public int RebuildsStartedThisTick;
    }
}
