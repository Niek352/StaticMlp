using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public static class SpawnSourceReachabilityRules
    {
        private const float REACHABLE_RECHECK_SECONDS = 2f;
        private const float UNREACHABLE_RECHECK_SECONDS = 1f;
        private const float TEMPORARILY_BLOCKED_RECHECK_SECONDS = 0.25f;
        private const float WAITING_FOR_NAV_MESH_RECHECK_SECONDS = 0.5f;

        public static SpawnSourceNavState CreateInitialState()
        {
            return new SpawnSourceNavState
            {
                Status = SpawnSourceNavStatus.Unknown,
                LastCheckedTime = -1f,
                NextCheckTime = 0f,
                ApproxPathCost = 0f,
                NavVersion = 0
            };
        }

        public static bool ContainsSource(in NavInterestArea navArea, float3 sourcePosition)
        {
            var distanceSq = math.distancesq(navArea.Center, sourcePosition);
            return distanceSq <= navArea.SourceCollectRadius * navArea.SourceCollectRadius;
        }

        public static bool IsBetterAreaCandidate(
            bool hasCurrentBest,
            int candidatePriority,
            float candidateDistanceSq,
            int currentBestPriority,
            float currentBestDistanceSq)
        {
            return !hasCurrentBest
                   || candidatePriority > currentBestPriority
                   || candidatePriority == currentBestPriority && candidateDistanceSq < currentBestDistanceSq;
        }

        public static bool NeedsReachabilityCheck(in SpawnSourceNavState state, int navVersion, float currentTime)
        {
            return state.Status == SpawnSourceNavStatus.Unknown
                   || state.NavVersion != navVersion
                   || currentTime >= state.NextCheckTime;
        }

        public static SpawnSourceReachabilityRequest CreateRequest(int cellId, int navVersion, float currentTime, float approxPathCost)
        {
            return new SpawnSourceReachabilityRequest
            {
                CellId = cellId,
                NavVersion = navVersion,
                RequestedAtTime = currentTime,
                ApproxPathCost = approxPathCost
            };
        }

        public static SpawnSourceNavState CreateDeferredState(
            in SpawnSourceNavState currentState,
            in RuntimeNavMeshZoneState zoneState,
            float currentTime,
            float approxPathCost)
        {
            return new SpawnSourceNavState
            {
                Status = zoneState.BuildState == RuntimeNavMeshBuildState.Ready
                    ? SpawnSourceNavStatus.TemporarilyBlocked
                    : SpawnSourceNavStatus.WaitingForNavMesh,
                LastCheckedTime = currentState.LastCheckedTime,
                NextCheckTime = currentTime + GetRecheckInterval(
                    zoneState.BuildState == RuntimeNavMeshBuildState.Ready
                        ? SpawnSourceNavStatus.TemporarilyBlocked
                        : SpawnSourceNavStatus.WaitingForNavMesh),
                ApproxPathCost = approxPathCost,
                NavVersion = zoneState.NavVersion
            };
        }

        public static SpawnSourceNavState CreateCheckedState(
            SpawnSourceNavStatus status,
            int navVersion,
            float currentTime,
            float approxPathCost)
        {
            return new SpawnSourceNavState
            {
                Status = status,
                LastCheckedTime = currentTime,
                NextCheckTime = currentTime + GetRecheckInterval(status),
                ApproxPathCost = approxPathCost,
                NavVersion = navVersion
            };
        }

        public static SpawnSourceNavState CreateOutOfAreaState(float currentTime)
        {
            return CreateCheckedState(SpawnSourceNavStatus.Unreachable, 0, currentTime, float.PositiveInfinity);
        }

        private static float GetRecheckInterval(SpawnSourceNavStatus status)
        {
            return status switch
            {
                SpawnSourceNavStatus.Reachable => REACHABLE_RECHECK_SECONDS,
                SpawnSourceNavStatus.Unreachable => UNREACHABLE_RECHECK_SECONDS,
                SpawnSourceNavStatus.TemporarilyBlocked => TEMPORARILY_BLOCKED_RECHECK_SECONDS,
                _ => WAITING_FOR_NAV_MESH_RECHECK_SECONDS
            };
        }
    }
}
