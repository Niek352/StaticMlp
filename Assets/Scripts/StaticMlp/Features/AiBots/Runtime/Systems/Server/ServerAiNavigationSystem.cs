using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiNavigationSystem : ISystem
    {
        private const float POSITION_EPSILON = 0.0001f;
        private const float VELOCITY_EPSILON = 0.0001f;
        private const float ROTATION_DOT_THRESHOLD = 0.9999f;

        public void Update()
        {
            if (!SW.HasResource<AiNavigationRuntime>())
                throw new InvalidOperationException("AI navigation runtime resource is missing in server world.");

            var runtime = SW.GetResource<AiNavigationRuntime>();
            if (runtime == null)
                throw new InvalidOperationException("AI navigation runtime resource exists but is null.");

            runtime.BeginFrame();

            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, CharacterNetState>>().Entities())
                UpdateAgent(runtime, entity);

            runtime.RemoveInactiveAgents();
        }

        private static void UpdateAgent(AiNavigationRuntime runtime, SW.Entity entity)
        {
            ref readonly var currentState = ref entity.Read<CharacterNetState>();
            var hasMoveRequest = entity.Has<AiMoveRequest>();
            Vector3 destination;
            float stopDistance;
            if (hasMoveRequest)
            {
                ref readonly var moveRequest = ref entity.Read<AiMoveRequest>();
                destination = moveRequest.Destination;
                stopDistance = moveRequest.StopDistance;
            }
            else
            {
                destination = currentState.Position;
                stopDistance = 0f;
            }

            runtime.SyncAgent(new AiNavigationRuntime.AgentInput(
                entity.GID,
                currentState.Position,
                currentState.Rotation,
                hasMoveRequest,
                destination,
                stopDistance));

            if (!runtime.TryGetResult(entity.GID, out var result))
                return;

            ApplyNavigationResult(entity, in currentState, result);

            if (hasMoveRequest && result.HasArrived && entity.Has<AiMoveRequest>())
                entity.Delete<AiMoveRequest>();
        }

        private static void ApplyNavigationResult(
            SW.Entity entity,
            in CharacterNetState currentState,
            AiNavigationRuntime.AgentResult result)
        {
            if (!HasStateChanged(in currentState, result))
                return;

            ref var replicatedState = ref ReplicationMut.Mut<CharacterNetState>(entity);
            replicatedState.Position = result.Position;
            replicatedState.Velocity = result.Velocity;
            replicatedState.Rotation = result.Rotation;
        }

        private static bool HasStateChanged(
            in CharacterNetState currentState,
            AiNavigationRuntime.AgentResult result)
        {
            if ((currentState.Position - result.Position).sqrMagnitude > POSITION_EPSILON)
                return true;

            if ((currentState.Velocity - result.Velocity).sqrMagnitude > VELOCITY_EPSILON)
                return true;

            return Mathf.Abs(Quaternion.Dot(currentState.Rotation, result.Rotation)) < ROTATION_DOT_THRESHOLD;
        }
    }
}
