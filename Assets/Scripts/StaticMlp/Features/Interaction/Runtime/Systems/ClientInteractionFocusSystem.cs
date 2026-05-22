using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Player;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Publishes the interactable under the aim ray first; if no aim target matches,
    /// falls back to the closest interactable within range.
    /// </summary>
    public sealed class ClientInteractionFocusSystem : ISystem
    {
        private const float FOCUS_RANGE          = 4f;
        private const float MIN_FOCUS_RADIUS     = 0.75f;
        private const float AIM_DISTANCE_EPSILON = 0.001f;

        public void Update()
        {
            ref var focus = ref CW.GetResource<InteractionFocus>();
            focus.Clear();

            if (!ClientLocalPlayer.TryGetPosition(out var playerPosition))
                return;

            var inputState = CW.GetResource<ClientInputState>();
            var hasAimRay = inputState.TryGetAimRay(out var aimRay);

            if (hasAimRay && TryFindAimTarget(in aimRay, playerPosition, out var aimTarget, out var aimKind, out var aimDistance))
            {
                focus.Target = aimTarget;
                focus.Kind = aimKind;
                focus.Distance = aimDistance;
                return;
            }

            if (TryFindNearestTarget(playerPosition, out var nearestTarget, out var nearestKind, out var nearestDistance))
            {
                focus.Target = nearestTarget;
                focus.Kind = nearestKind;
                focus.Distance = nearestDistance;
            }
        }

        private static bool TryFindAimTarget(
            in Ray aimRay,
            Vector3 playerPosition,
            out EntityGID target,
            out InteractableKind kind,
            out float distance)
        {
            var bestRayDistance = float.MaxValue;
            var bestPlayerDistanceSq = FOCUS_RANGE * FOCUS_RANGE;
            var found = false;
            target = default;
            kind = InteractableKind.None;
            distance = 0f;

            foreach (var entity in CW.Query<All<InteractableTag, Interactable, InteractableFocusPoint>>().Entities())
            {
                ref readonly var point = ref entity.Read<InteractableFocusPoint>();
                var playerDistanceSq = (point.Position - playerPosition).sqrMagnitude;
                if (playerDistanceSq > FOCUS_RANGE * FOCUS_RANGE)
                    continue;

                if (!AimRayHitsPoint(in aimRay, in point, out var rayDistance))
                    continue;

                if (rayDistance > bestRayDistance + AIM_DISTANCE_EPSILON)
                    continue;

                if (Mathf.Abs(rayDistance - bestRayDistance) <= AIM_DISTANCE_EPSILON
                    && playerDistanceSq >= bestPlayerDistanceSq)
                    continue;

                bestRayDistance = rayDistance;
                bestPlayerDistanceSq = playerDistanceSq;
                target = entity.GID;
                kind = entity.Read<Interactable>().Kind;
                distance = Mathf.Sqrt(playerDistanceSq);
                found = true;
            }

            return found;
        }

        private static bool TryFindNearestTarget(
            Vector3 playerPosition,
            out EntityGID target,
            out InteractableKind kind,
            out float distance)
        {
            var bestDistanceSq = FOCUS_RANGE * FOCUS_RANGE;
            var found = false;
            target = default;
            kind = InteractableKind.None;
            distance = 0f;

            foreach (var entity in CW.Query<All<InteractableTag, Interactable, InteractableFocusPoint>>().Entities())
            {
                ref readonly var point = ref entity.Read<InteractableFocusPoint>();
                var distanceSq = (point.Position - playerPosition).sqrMagnitude;
                if (distanceSq > bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                target = entity.GID;
                kind = entity.Read<Interactable>().Kind;
                distance = Mathf.Sqrt(distanceSq);
                found = true;
            }

            return found;
        }

        private static bool AimRayHitsPoint(
            in Ray aimRay,
            in InteractableFocusPoint point,
            out float rayDistance)
        {
            var toPoint = point.Position - aimRay.origin;
            rayDistance = Vector3.Dot(toPoint, aimRay.direction);
            if (rayDistance < 0f)
                return false;

            var closestPoint = aimRay.origin + aimRay.direction * rayDistance;
            var radius = Mathf.Max(point.Radius, MIN_FOCUS_RADIUS);
            return (point.Position - closestPoint).sqrMagnitude <= radius * radius;
        }
    }
}
