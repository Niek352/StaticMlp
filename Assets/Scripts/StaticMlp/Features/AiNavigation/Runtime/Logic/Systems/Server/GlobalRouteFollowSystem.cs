using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Game;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class GlobalRouteFollowSystem : ISystem
    {
        private const float CHUNK_WORLD_SIZE = 128f;
        private readonly List<int2> _path = new();

        public void Update()
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            var deltaTime = simulationTime.FixedStepSeconds;
            var registry = SW.GetResource<ChunkNavSourceRegistry>();
            var graph = SW.GetResource<GlobalNavGraph>();

            foreach (var entity in SW.Query<All<AiNavigationModeState, AiFarSimulationState, GlobalRoute>>().Entities())
                FollowRoute(entity, deltaTime, registry, graph);
        }

        private void FollowRoute(SW.Entity entity, float deltaTime, ChunkNavSourceRegistry registry, GlobalNavGraph graph)
        {
            ref readonly var modeState = ref entity.Read<AiNavigationModeState>();
            if (!FarAiMovementRules.IsFarSimulationMode(modeState.CurrentMode))
                return;

            ref var farState = ref entity.Mut<AiFarSimulationState>();
            ref var route = ref entity.Mut<GlobalRoute>();

            route.RepathTimer -= deltaTime;
            if (route.RepathTimer > 0f)
                return;

            route.RepathTimer = route.RepathInterval;

            var currentChunk = WorldChunkId.FromWorldPosition(
                farState.LogicalPosition.x,
                farState.LogicalPosition.z,
                CHUNK_WORLD_SIZE);

            var targetChunk = WorldChunkId.FromWorldPosition(
                route.FinalTargetPosition.x,
                route.FinalTargetPosition.z,
                CHUNK_WORLD_SIZE);

            var currentCoord = new int2(currentChunk.X, currentChunk.Z);
            var targetCoord = new int2(targetChunk.X, targetChunk.Z);

            if (currentCoord.Equals(targetCoord))
            {
                farState.TargetPosition = route.FinalTargetPosition;
                return;
            }

            _path.Clear();
            var hasPath = graph.TryFindPath(currentCoord, targetCoord, registry, _path);

            if (hasPath && _path.Count >= 2)
            {
                var nextChunk = new WorldChunkId(_path[1].x, _path[1].y);
                var nextCenter = nextChunk.GetWorldCenter(CHUNK_WORLD_SIZE);
                farState.TargetPosition = new float3(nextCenter.x, farState.LogicalPosition.y, nextCenter.z);
            }
            else
            {
                // Fallback: move directly to final target
                farState.TargetPosition = route.FinalTargetPosition;
            }
        }
    }
}
