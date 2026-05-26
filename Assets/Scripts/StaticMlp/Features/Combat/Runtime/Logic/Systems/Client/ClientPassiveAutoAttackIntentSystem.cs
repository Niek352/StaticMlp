using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackIntentSystem : ISystem
    {
        private readonly List<EntityGID> _players = new();

        public void Update()
        {
            var config = CW.GetResource<CombatConfig>();
            var now = CW.GetResource<GameTime>().Time;
            _players.Clear();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState, PassiveAutoAttackState>>().Entities())
                _players.Add(player.GID);

            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].TryUnpack<ClientCoreWT>(out var player))
                    UpdatePlayer(player, config, now);
            }
        }

        private static void UpdatePlayer(CW.Entity player, CombatConfig config, float now)
        {
            ref var state = ref player.Mut<PassiveAutoAttackState>();
            var abilityId = player.Read<PreparedLoadoutSnapshot>().PreparedAbilityId;
            if (!IsValidTarget(state.CurrentTarget))
            {
                if (player.Has<PassiveAutoAttackIntent>())
                    player.Delete<PassiveAutoAttackIntent>();
                return;
            }

            if (now < state.NextFireAt)
                return;

            state.LastShotSequence++;
            state.NextFireAt = now + Mathf.Max(0f, GetCooldown(abilityId, config));

            player.Set(new PassiveAutoAttackIntent
            {
                AbilityId = abilityId,
                Target = state.CurrentTarget,
                ShotSequence = state.LastShotSequence,
                LocalFireTime = now
            });

            if (!player.Has<LocalCombatPredictionState>())
                player.Set(new LocalCombatPredictionState());

            ref var prediction = ref player.Mut<LocalCombatPredictionState>();
            prediction.LastPredictedCommandId = state.LastShotSequence;
        }

        private static float GetCooldown(CombatAbilityId abilityId, CombatConfig config)
        {
            switch (abilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    return config.PoisonArrowCooldown;
                case CombatAbilityId.FireFlask:
                    return config.FireFlaskCooldown;
                default:
                    return config.FireInterval;
            }
        }

        private static bool IsValidTarget(EntityGID target)
        {
            return target.Raw != 0ul && target.TryUnpack<ClientCoreWT>(out _);
        }
    }
}
