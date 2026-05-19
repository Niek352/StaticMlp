using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Shared;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackTargetingSystem : ISystem
    {
        private readonly List<EntityGID> _players = new();

        public void Update()
        {
            var config = CW.GetResource<CombatConfig>();
            var now = CW.GetResource<GameTime>().Time;
            _players.Clear();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
                _players.Add(player.GID);

            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].TryUnpack<ClientCoreWT>(out var player))
                    UpdatePlayer(player, config, now);
            }
        }

        private static void UpdatePlayer(CW.Entity player, CombatConfig config, float now)
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
            state.CurrentTarget = FindNearestTarget(player, GetRange(abilityId, config));
            if (previousTarget.Raw != 0ul && state.CurrentTarget.Raw == 0ul)
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

        private static EntityGID FindNearestTarget(CW.Entity player, float radius)
        {
            var playerPosition = player.Read<CharacterNetState>().Position;
            var radiusSq = radius * radius;
            var hasBest = false;
            var bestDistanceSq = 0f;
            EntityGID bestTarget = default;

            foreach (var candidate in CW.Query<All<MonsterTag, CharacterNetState>>().Entities())
            {
                if (candidate.Has<Health>() && candidate.Read<Health>().Current <= 0f)
                    continue;

                var delta = candidate.Read<CharacterNetState>().Position - playerPosition;
                var distanceSq = delta.sqrMagnitude;
                if (distanceSq > radiusSq)
                    continue;

                if (!hasBest
                    || distanceSq < bestDistanceSq
                    || (Mathf.Approximately(distanceSq, bestDistanceSq) && candidate.GID.Raw < bestTarget.Raw))
                {
                    hasBest = true;
                    bestDistanceSq = distanceSq;
                    bestTarget = candidate.GID;
                }
            }

            return bestTarget;
        }
    }
}
