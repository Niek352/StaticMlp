using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiNavigation;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using Unity.Mathematics;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiLocalNavigationHandoffApplySystem : ISystem
    {
        private EventReceiver<ServerWT, LocalNavigationHandoffRequestEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<LocalNavigationHandoffRequestEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var request in _requests)
                TryApply(in request.Value);
        }

        private static void TryApply(in LocalNavigationHandoffRequestEvent request)
        {
            if (!request.Entity.TryUnpack<ServerWT>(out var entity))
                throw new InvalidOperationException("Local navigation handoff target is not available in the server world.");

            if (!entity.Has<AiAgentTag>())
                return;

            if (!entity.Has<CharacterNetState>())
                throw new InvalidOperationException("AI local navigation handoff requires CharacterNetState on the target entity.");

            ApplyProjectedPosition(entity, request.LogicalPosition);
            ApplyMoveRequest(entity, request.LogicalPosition, request.Destination, request.StopDistance);
        }

        private static void ApplyProjectedPosition(SW.Entity entity, float3 logicalPosition)
        {
            ref readonly var currentState = ref entity.Read<CharacterNetState>();
            var projectedPosition = new Vector3(logicalPosition.x, logicalPosition.y, logicalPosition.z);
            if (currentState.Position == projectedPosition && currentState.Velocity == Vector3.zero)
                return;

            ref var replicatedState = ref ReplicationMut.Mut<CharacterNetState>(entity);
            replicatedState.Position = projectedPosition;
            replicatedState.Velocity = Vector3.zero;
        }

        private static void ApplyMoveRequest(
            SW.Entity entity,
            float3 logicalPosition,
            float3 destination,
            float stopDistance)
        {
            var destinationVector = new Vector3(destination.x, destination.y, destination.z);
            var logicalPositionVector = new Vector3(logicalPosition.x, logicalPosition.y, logicalPosition.z);
            var effectiveStopDistance = Mathf.Max(stopDistance, 0f);
            if ((destinationVector - logicalPositionVector).sqrMagnitude <= effectiveStopDistance * effectiveStopDistance)
            {
                if (entity.Has<AiMoveRequest>())
                    entity.Delete<AiMoveRequest>();

                return;
            }

            var moveRequest = new AiMoveRequest
            {
                Destination = destinationVector,
                StopDistance = effectiveStopDistance
            };

            if (entity.Has<AiMoveRequest>())
            {
                entity.Mut<AiMoveRequest>() = moveRequest;
                return;
            }

            entity.Set(moveRequest);
        }
    }
}
