using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.CombatDirector;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StaticMlp.Features.AiNavigation
{
    public sealed class AiNavigationDebugGizmoPart : MonoBehaviour, IEntityViewPart
    {
        [SerializeField] private bool _showInPlayModeOnly = true;
        [SerializeField] private bool _showText = true;
        [SerializeField] private Color _navAreaColor = new(0.16f, 0.92f, 0.52f, 1f);
        [SerializeField] private Color _bestSourceColor = new(1f, 0.78f, 0.18f, 1f);
        [SerializeField] private Color _resolvedSourceColor = new(1f, 0.34f, 0.22f, 1f);
        [SerializeField] private Color _pathGoalColor = new(0.18f, 0.72f, 1f, 1f);
        [SerializeField] private float _bestSourceRadius = 0.2f;
        [SerializeField] private float _resolvedSourceRadius = 0.28f;
        [SerializeField] private float _labelHeight = 2.4f;

        private EntityGID _gid;
        private bool _isBound;

        public void OnBind(IEntityView view)
        {
            _gid = view.Entity.GID;
            _isBound = true;
        }

        public void OnUnbind()
        {
            _gid = default;
            _isBound = false;
        }

        private void OnDrawGizmos()
        {
            if (!_isBound)
                return;

            if (_showInPlayModeOnly && !Application.isPlaying)
                return;

            if (SW.Status != WorldStatus.Initialized)
                return;

            if (!_gid.TryUnpack<ServerWT>(out var entity))
                return;

            var position = ResolveDebugPosition(entity);
            if (!TryResolveSnapshot(position, out var snapshot))
                return;

            DrawNavArea(snapshot);
            DrawBestSource(snapshot);
            DrawResolvedSource(snapshot, position);
            DrawGoal(snapshot, entity, position);
            DrawLabel(snapshot);
        }

        private static float3 ResolveDebugPosition(SW.Entity entity)
        {
            if (entity.Has<AiFarSimulationState>())
                return entity.Read<AiFarSimulationState>().LogicalPosition;

            return entity.Read<CharacterNetState>().Position;
        }

        private static bool TryResolveSnapshot(float3 position, out AiNavigationDebugSnapshot snapshot)
        {
            snapshot = default;

            var found = false;
            var bestPriority = int.MinValue;
            var bestDistanceSq = float.MaxValue;

            foreach (var entity in SW.Query<All<NavInterestArea, AiNavigationDebugSnapshot>>().Entities())
            {
                ref readonly var navArea = ref entity.Read<NavInterestArea>();
                if (!SpawnSourceReachabilityRules.ContainsSource(in navArea, position))
                    continue;

                var distanceSq = math.distancesq(navArea.Center, position);
                if (!SpawnSourceReachabilityRules.IsBetterAreaCandidate(found, navArea.Priority, distanceSq, bestPriority, bestDistanceSq))
                    continue;

                snapshot = entity.Read<AiNavigationDebugSnapshot>();
                bestPriority = navArea.Priority;
                bestDistanceSq = distanceSq;
                found = true;
            }

            return found;
        }

        private void DrawNavArea(in AiNavigationDebugSnapshot snapshot)
        {
            Gizmos.color = _navAreaColor;
            var center = ToVector3(snapshot.NavAreaCenter);
            Gizmos.DrawWireSphere(center, snapshot.NavAreaRadius);
        }

        private void DrawBestSource(in AiNavigationDebugSnapshot snapshot)
        {
            if (!snapshot.HasBestReachableSource)
                return;

            if (!snapshot.BestReachableSource.TryUnpack<ServerWT>(out var sourceEntity))
                return;

            if (!sourceEntity.Has<SpawnSource>())
                return;

            var sourcePosition = ToVector3(sourceEntity.Read<SpawnSource>().Position);
            Gizmos.color = _bestSourceColor;
            Gizmos.DrawWireSphere(sourcePosition, _bestSourceRadius);
        }

        private void DrawResolvedSource(in AiNavigationDebugSnapshot snapshot, float3 botPosition)
        {
            if (!snapshot.HasResolvedReachableSource)
                return;

            var resolvedPosition = ToVector3(snapshot.ResolvedReachablePosition);
            Gizmos.color = _resolvedSourceColor;
            Gizmos.DrawWireSphere(resolvedPosition, _resolvedSourceRadius);
            Gizmos.DrawLine(ToVector3(botPosition), resolvedPosition);
        }

        private void DrawGoal(in AiNavigationDebugSnapshot snapshot, SW.Entity entity, float3 botPosition)
        {
            if (!SW.HasResource<AiNavigationRuntime>())
                return;

            var runtime = SW.GetResource<AiNavigationRuntime>();
            if (runtime == null || !runtime.TryGetDebugState(entity.GID, out var debugState) || !debugState.HasGoal)
                return;

            Gizmos.color = _pathGoalColor;
            Gizmos.DrawLine(ToVector3(botPosition), debugState.Goal);
            Gizmos.DrawWireSphere(debugState.Goal, _bestSourceRadius);
        }

        private void DrawLabel(in AiNavigationDebugSnapshot snapshot)
        {
            if (!_showText)
                return;

#if UNITY_EDITOR
            var center = ToVector3(snapshot.NavAreaCenter) + Vector3.up * _labelHeight;
            Handles.Label(center, BuildLabel(snapshot));
#endif
        }

        private static string BuildLabel(in AiNavigationDebugSnapshot snapshot)
        {
            var phaseText = snapshot.HasDirectorPhase
                ? $"{snapshot.DirectorPhase} ({snapshot.DirectorPhaseId})"
                : "n/a";

            return
                $"AiNavigation Area {snapshot.NavAreaId}\n" +
                $"Build: {snapshot.NavBuildState} | NavVersion: {snapshot.NavVersion}/{snapshot.RequestedNavVersion}\n" +
                $"Director Phase: {phaseText}\n" +
                $"Sources R/U/P: {snapshot.ReachableSourceCount}/{snapshot.UnreachableSourceCount}/{snapshot.PendingSourceCount}\n" +
                $"Alive Enemies: {snapshot.AliveEnemyCount}/{snapshot.MaxAliveEnemies}\n" +
                $"Work Tick P/R/Q/S: {snapshot.PathRequestsThisTick}/{snapshot.ReachabilityChecksThisTick}/{snapshot.RebuildRequestsQueuedThisTick}/{snapshot.RebuildsStartedThisTick}";
        }

        private static Vector3 ToVector3(float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }
    }
}
