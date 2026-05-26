using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceHarvestInputSystem : ISystem
    {
        private const float AIM_CONE_DEGREES = 12f;
        private const float HIT_POINT_QUANTIZATION = 0.01f;
        private const float HARVEST_INTERACTION_RANGE = 4f;
        private const ushort DEFAULT_TOOL_ID = 0;

        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            if (!inputState.WasPressed(CoreInputActions.Primary))
                return;

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                if (!TryCreateCommand(player, inputState, out var command))
                    continue;

                CW.SendToServer(in command);
            }
        }

        private static bool TryCreateCommand(CW.Entity player, ClientInputState inputState, out TryHarvestOpenWorldResourceCommand command)
        {
            if (inputState.TryGetAimRay(out var aimRay))
            {
                if (TryFindAimTarget(player, aimRay, out command))
                    return true;

                return TryFindNearestTarget(player, out command);
            }

            return TryFindNearestTarget(player, out command);
        }

        private static bool TryFindAimTarget(CW.Entity player, Ray aimRay, out TryHarvestOpenWorldResourceCommand command)
        {
            var direction = aimRay.direction;
            if (!IsFinite(aimRay.origin)
                || !IsFinite(direction)
                || direction.sqrMagnitude <= 0.0001f)
            {
                command = default;
                return false;
            }

            direction.Normalize();
            var aim = new AimSelection(aimRay.origin, direction, Mathf.Cos(AIM_CONE_DEGREES * Mathf.Deg2Rad));
            var playerPosition = player.Read<CharacterNetState>().Position;
            var radiusSq = HARVEST_INTERACTION_RANGE * HARVEST_INTERACTION_RANGE;
            var hasBest = false;
            var bestAimDistanceSq = 0f;
            var bestSourceDistanceSq = 0f;
            var bestTieBreaker = ulong.MaxValue;
            var bestCommand = default(TryHarvestOpenWorldResourceCommand);

            foreach (var candidate in CW.Query<All<OpenWorldResourceTargetable, OpenWorldResourceTargetState>>().Entities())
            {
                ref readonly var resource = ref candidate.Read<OpenWorldResourceTargetState>();
                if (!IsTargetableResource(resource))
                    continue;

                if (!TryGetCandidateScore(
                        playerPosition,
                        resource.WorldPosition,
                        radiusSq,
                        aim,
                        out var aimDistanceSq,
                        out var sourceDistanceSq))
                {
                    continue;
                }

                var tieBreaker = DeterministicPlacementTieBreaker(resource.PlacementId);
                if (IsBetterAimCandidate(
                        hasBest,
                        aimDistanceSq,
                        bestAimDistanceSq,
                        sourceDistanceSq,
                        bestSourceDistanceSq,
                        tieBreaker,
                        bestTieBreaker))
                {
                    hasBest = true;
                    bestAimDistanceSq = aimDistanceSq;
                    bestSourceDistanceSq = sourceDistanceSq;
                    bestTieBreaker = tieBreaker;
                    bestCommand = CreateCommand(resource);
                }
            }

            command = bestCommand;
            return hasBest;
        }

        private static bool TryFindNearestTarget(CW.Entity player, out TryHarvestOpenWorldResourceCommand command)
        {
            var playerPosition = player.Read<CharacterNetState>().Position;
            var radiusSq = HARVEST_INTERACTION_RANGE * HARVEST_INTERACTION_RANGE;
            var hasBest = false;
            var bestDistanceSq = 0f;
            var bestTieBreaker = ulong.MaxValue;
            var bestCommand = default(TryHarvestOpenWorldResourceCommand);

            foreach (var candidate in CW.Query<All<OpenWorldResourceTargetable, OpenWorldResourceTargetState>>().Entities())
            {
                ref readonly var resource = ref candidate.Read<OpenWorldResourceTargetState>();
                if (!IsTargetableResource(resource))
                    continue;

                var delta = resource.WorldPosition - playerPosition;
                var distanceSq = delta.sqrMagnitude;
                if (distanceSq > radiusSq)
                    continue;

                var tieBreaker = DeterministicPlacementTieBreaker(resource.PlacementId);
                if (!hasBest
                    || distanceSq < bestDistanceSq
                    || (Mathf.Approximately(distanceSq, bestDistanceSq) && tieBreaker < bestTieBreaker))
                {
                    hasBest = true;
                    bestDistanceSq = distanceSq;
                    bestTieBreaker = tieBreaker;
                    bestCommand = CreateCommand(resource);
                }
            }

            command = bestCommand;
            return hasBest;
        }

        private static bool TryGetCandidateScore(
            Vector3 playerPosition,
            Vector3 targetPosition,
            float radiusSq,
            AimSelection aim,
            out float aimDistanceSq,
            out float sourceDistanceSq)
        {
            var fromPlayer = targetPosition - playerPosition;
            sourceDistanceSq = fromPlayer.sqrMagnitude;
            if (sourceDistanceSq > radiusSq)
            {
                aimDistanceSq = 0f;
                return false;
            }

            var fromRay = targetPosition - aim.Origin;
            var projectedDistance = Vector3.Dot(fromRay, aim.Direction);
            if (projectedDistance <= 0f)
            {
                aimDistanceSq = 0f;
                return false;
            }

            var candidateDirection = fromRay.normalized;
            if (Vector3.Dot(aim.Direction, candidateDirection) < aim.MinDot)
            {
                aimDistanceSq = 0f;
                return false;
            }

            aimDistanceSq = Mathf.Max(0f, fromRay.sqrMagnitude - projectedDistance * projectedDistance);
            return true;
        }

        private static bool IsBetterAimCandidate(
            bool hasBest,
            float aimDistanceSq,
            float bestAimDistanceSq,
            float sourceDistanceSq,
            float bestSourceDistanceSq,
            ulong tieBreaker,
            ulong bestTieBreaker)
        {
            return !hasBest
                   || aimDistanceSq < bestAimDistanceSq
                   || (Mathf.Approximately(aimDistanceSq, bestAimDistanceSq)
                       && (sourceDistanceSq < bestSourceDistanceSq
                           || (Mathf.Approximately(sourceDistanceSq, bestSourceDistanceSq)
                               && tieBreaker < bestTieBreaker)));
        }

        private static TryHarvestOpenWorldResourceCommand CreateCommand(in OpenWorldResourceTargetState resource)
        {
            return new TryHarvestOpenWorldResourceCommand
            {
                PlacementId = resource.PlacementId,
                ToolId = DEFAULT_TOOL_ID,
                HitPointXQ = QuantizeHitPoint(resource.WorldPosition.x),
                HitPointYQ = QuantizeHitPoint(resource.WorldPosition.y),
                HitPointZQ = QuantizeHitPoint(resource.WorldPosition.z)
            };
        }

        private static int QuantizeHitPoint(float value)
        {
            return Mathf.RoundToInt(value / HIT_POINT_QUANTIZATION);
        }

        private static bool IsTargetableResource(in OpenWorldResourceTargetState resource)
        {
            const OpenWorldResourceOverlayFlags inactive =
                OpenWorldResourceOverlayFlags.Depleted
                | OpenWorldResourceOverlayFlags.Hidden
                | OpenWorldResourceOverlayFlags.Replaced;

            return resource.PlacementId != 0L
                   && resource.RemainingAmount > 0
                   && (resource.Flags & inactive) == 0
                   && IsFinite(resource.WorldPosition);
        }

        private static ulong DeterministicPlacementTieBreaker(long placementId)
        {
            return unchecked((ulong)placementId);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private readonly struct AimSelection
        {
            public readonly Vector3 Origin;
            public readonly Vector3 Direction;
            public readonly float MinDot;

            public AimSelection(Vector3 origin, Vector3 direction, float minDot)
            {
                Origin = origin;
                Direction = direction;
                MinDot = minDot;
            }
        }
    }
}
