using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Features.Shared;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackTargetingSystem : ISystem
    {
        private const float AIM_CONE_DEGREES = 12f;
        private const float HIT_POINT_QUANTIZATION = 0.01f;

        private readonly List<EntityGID> _players = new();

        public void Update()
        {
            var config = CW.GetResource<CombatConfig>();
            var inputState = CW.GetResource<ClientInputState>();
            var now = CW.GetResource<GameTime>().Time;
            _players.Clear();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
                _players.Add(player.GID);

            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].TryUnpack<ClientCoreWT>(out var player))
                    UpdatePlayer(player, config, inputState, now);
            }
        }

        private static void UpdatePlayer(CW.Entity player, CombatConfig config, ClientInputState inputState, float now)
        {
            if (!player.Has<PassiveAutoAttackState>())
            {
                player.Set(new PassiveAutoAttackState
                {
                    NextFireAt = now
                });
            }

            ref var state = ref player.Mut<PassiveAutoAttackState>();
            var previousTarget = state.CurrentTarget;
            var abilityId = player.Read<PreparedLoadoutSnapshot>().PreparedAbilityId;
            var range = GetRange(abilityId, config);
            state.CurrentTarget = inputState.TryGetAimRay(out var aimRay)
                ? FindAimTargetOrNearest(player, range, aimRay)
                : FindNearestTarget(player, range);

            if (previousTarget.Kind != CombatTargetKind.None && state.CurrentTarget.Kind == CombatTargetKind.None)
                state.NextFireAt = now;
        }

        private static float GetRange(CombatAbilityId abilityId, CombatConfig config)
        {
            switch (abilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    return config.PoisonArrowRange;
                case CombatAbilityId.FireFlask:
                    return config.FireFlaskRange;
                default:
                    return config.Radius;
            }
        }

        private static CombatTargetRef FindAimTargetOrNearest(CW.Entity player, float radius, Ray aimRay)
        {
            var direction = aimRay.direction;
            if (!IsFinite(aimRay.origin)
                || !IsFinite(direction)
                || direction.sqrMagnitude <= 0.0001f)
            {
                return FindNearestTarget(player, radius);
            }

            direction.Normalize();
            var aim = new AimSelection(aimRay.origin, direction, Mathf.Cos(AIM_CONE_DEGREES * Mathf.Deg2Rad));
            if (TryFindAimTarget(player, radius, aim, out var target))
                return target;

            return FindNearestTarget(player, radius);
        }

        private static bool TryFindAimTarget(CW.Entity player, float radius, AimSelection aim, out CombatTargetRef target)
        {
            var playerPosition = player.Read<CharacterNetState>().Position;
            var radiusSq = radius * radius;
            var hasBest = false;
            var bestAimDistanceSq = 0f;
            var bestSourceDistanceSq = 0f;
            var bestTieBreaker = ulong.MaxValue;
            var bestTarget = default(CombatTargetRef);

            foreach (var candidate in CW.Query<All<MonsterTag, CharacterNetState>>().Entities())
            {
                if (!candidate.Has<Health>() || candidate.Read<Health>().Current <= 0f)
                    continue;

                var targetPosition = candidate.Read<CharacterNetState>().Position;
                if (!TryGetCandidateScore(
                        playerPosition,
                        targetPosition,
                        radiusSq,
                        aim,
                        out var aimDistanceSq,
                        out var sourceDistanceSq))
                {
                    continue;
                }

                var tieBreaker = candidate.GID.Raw;
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
                    bestTarget = new CombatTargetRef
                    {
                        Kind = CombatTargetKind.ActorEntity,
                        Entity = candidate.GID
                    };
                }
            }

            foreach (var candidate in CW.Query<All<OpenWorldResourceTargetable, OpenWorldResourceTargetState>>().Entities())
            {
                ref readonly var resource = ref candidate.Read<OpenWorldResourceTargetState>();
                if (!IsActiveResourceTarget(resource))
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
                    bestTarget = CreateStaticPlacementTarget(resource);
                }
            }

            target = bestTarget;
            return hasBest;
        }

        private static CombatTargetRef FindNearestTarget(CW.Entity player, float radius)
        {
            var playerPosition = player.Read<CharacterNetState>().Position;
            var radiusSq = radius * radius;
            var hasBest = false;
            var bestDistanceSq = 0f;
            var bestTieBreaker = ulong.MaxValue;
            var bestTarget = default(CombatTargetRef);

            foreach (var candidate in CW.Query<All<MonsterTag, CharacterNetState>>().Entities())
            {
                if (!candidate.Has<Health>() || candidate.Read<Health>().Current <= 0f)
                    continue;

                var delta = candidate.Read<CharacterNetState>().Position - playerPosition;
                var distanceSq = delta.sqrMagnitude;
                if (distanceSq > radiusSq)
                    continue;

                var tieBreaker = candidate.GID.Raw;
                if (!hasBest
                    || distanceSq < bestDistanceSq
                    || (Mathf.Approximately(distanceSq, bestDistanceSq) && tieBreaker < bestTieBreaker))
                {
                    hasBest = true;
                    bestDistanceSq = distanceSq;
                    bestTieBreaker = tieBreaker;
                    bestTarget = new CombatTargetRef
                    {
                        Kind = CombatTargetKind.ActorEntity,
                        Entity = candidate.GID
                    };
                }
            }

            foreach (var candidate in CW.Query<All<OpenWorldResourceTargetable, OpenWorldResourceTargetState>>().Entities())
            {
                ref readonly var resource = ref candidate.Read<OpenWorldResourceTargetState>();
                if (!IsActiveResourceTarget(resource))
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
                    bestTarget = CreateStaticPlacementTarget(resource);
                }
            }

            return hasBest ? bestTarget : default;
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

        private static CombatTargetRef CreateStaticPlacementTarget(in OpenWorldResourceTargetState resource)
        {
            return new CombatTargetRef
            {
                Kind = CombatTargetKind.StaticPlacement,
                PlacementId = resource.PlacementId,
                HitPointXQ = QuantizeHitPoint(resource.WorldPosition.x),
                HitPointYQ = QuantizeHitPoint(resource.WorldPosition.y),
                HitPointZQ = QuantizeHitPoint(resource.WorldPosition.z)
            };
        }

        private static int QuantizeHitPoint(float value)
        {
            return Mathf.RoundToInt(value / HIT_POINT_QUANTIZATION);
        }

        private static bool IsActiveResourceTarget(in OpenWorldResourceTargetState resource)
        {
            const OpenWorldResourceOverlayFlags inactive =
                OpenWorldResourceOverlayFlags.Depleted
                | OpenWorldResourceOverlayFlags.Hidden
                | OpenWorldResourceOverlayFlags.Replaced;

            return resource.PlacementId != 0L
                   && resource.RemainingAmount > 0
                   && (resource.Flags & inactive) == 0;
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
