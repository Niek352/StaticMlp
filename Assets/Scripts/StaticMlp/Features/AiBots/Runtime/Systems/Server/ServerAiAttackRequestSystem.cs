using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiAttackRequestSystem : ISystem
    {
        private readonly Func<float> _timeProvider;

        public ServerAiAttackRequestSystem(Func<float> timeProvider = null)
        {
            _timeProvider = timeProvider ?? (() => Time.time);
        }

        public void Update()
        {
            var config = RequireConfig();
            var now = _timeProvider();

            foreach (var attacker in SW.Query<All<ServerOwned, AiAgentTag, CharacterNetState, AiAttackRequest>>().Entities())
                TryAttack(attacker, config, now);
        }

        private static void TryAttack(SW.Entity attacker, CombatAutoAttackConfig config, float now)
        {
            ref readonly var request = ref attacker.Read<AiAttackRequest>();
            if (!request.Target.TryUnpack<ServerWT>(out var target)
                || target.GID == attacker.GID
                || !target.Has<CharacterNetState>()
                || !target.Has<Health>())
            {
                return;
            }

            if (target.Read<Health>().Current <= 0f)
                return;

            var attackerPosition = attacker.Read<CharacterNetState>().Position;
            var targetPosition = target.Read<CharacterNetState>().Position;
            if ((attackerPosition - targetPosition).sqrMagnitude > config.Radius * config.Radius)
                return;

            ref var state = ref EnsureAttackState(attacker);
            if (now < state.NextAttackAt)
                return;

            state.NextAttackAt = now + Mathf.Max(0f, config.FireInterval);
            EffectCommands.CreateDamage(
                attacker.GID,
                target.GID,
                config.DamageValue,
                DamageType.Physical);
        }

        private static ref ServerCombatAttackState EnsureAttackState(SW.Entity attacker)
        {
            if (!attacker.Has<ServerCombatAttackState>())
                attacker.Set(new ServerCombatAttackState());

            return ref attacker.Mut<ServerCombatAttackState>();
        }

        private static CombatAutoAttackConfig RequireConfig()
        {
            if (!SW.HasResource<CombatAutoAttackConfig>())
                throw new InvalidOperationException("Combat auto attack config resource is missing.");

            var config = SW.GetResource<CombatAutoAttackConfig>();
            if (config == null)
                throw new InvalidOperationException("Combat auto attack config resource is null.");

            return config;
        }
    }
}
