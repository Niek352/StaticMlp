using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackIntentSystem : ISystem
    {
        private readonly Func<float> _timeProvider;
        private readonly List<EntityGID> _players = new();

        public ClientPassiveAutoAttackIntentSystem(Func<float> timeProvider = null)
        {
            _timeProvider = timeProvider ?? (() => Time.time);
        }

        public void Update()
        {
            var config = CW.GetResource<CombatAutoAttackConfig>();
            _players.Clear();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState, PassiveAutoAttackState>>().Entities())
                _players.Add(player.GID);

            var now = _timeProvider();
            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].TryUnpack<ClientCoreWT>(out var player))
                    UpdatePlayer(player, config, now);
            }
        }

        private static void UpdatePlayer(CW.Entity player, CombatAutoAttackConfig config, float now)
        {
            ref var state = ref player.Mut<PassiveAutoAttackState>();
            if (state.CurrentTarget.Raw == 0ul || !state.CurrentTarget.TryUnpack<ClientCoreWT>(out _))
            {
                if (player.Has<PassiveAutoAttackIntent>())
                    player.Delete<PassiveAutoAttackIntent>();
                return;
            }

            if (now < state.NextFireAt)
                return;

            state.LastShotSequence++;
            state.NextFireAt = now + Mathf.Max(0f, config.FireInterval);

            player.Set(new PassiveAutoAttackIntent
            {
                Target = state.CurrentTarget,
                ShotSequence = state.LastShotSequence,
                LocalFireTime = now
            });
        }
    }
}
