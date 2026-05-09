using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackTargetingSystem : ISystem
    {
        private readonly Func<float> _timeProvider;
        private readonly List<EntityGID> _players = new();

        public ClientPassiveAutoAttackTargetingSystem(Func<float> timeProvider = null)
        {
            _timeProvider = timeProvider ?? (() => Time.time);
        }

        public void Update()
        {
            var config = CW.GetResource<CombatAutoAttackConfig>();
            var now = _timeProvider();
            _players.Clear();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
                _players.Add(player.GID);

            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].TryUnpack<ClientCoreWT>(out var player))
                    UpdatePlayer(player, config, now);
            }
        }

        private static void UpdatePlayer(CW.Entity player, CombatAutoAttackConfig config, float now)
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
            state.CurrentTarget = FindNearestTarget(player, config.Radius);
            if (previousTarget.Raw != 0ul && state.CurrentTarget.Raw == 0ul)
                state.NextFireAt = now;
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
