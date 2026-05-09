using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerDamageApplySystem : ISystem
    {
        private readonly List<EntityGID> _pendingEffects = new();

        public void Update()
        {
            _pendingEffects.Clear();

            foreach (var effect in SW.Query<All<EffectTag, DamageEffectTag, EffectTarget, EffectValue, DamageData>, None<EffectProcessedTag>>().Entities())
                _pendingEffects.Add(effect.GID);

            for (var i = 0; i < _pendingEffects.Count; i++)
            {
                if (_pendingEffects[i].TryUnpack<ServerWT>(out var effect)
                    && !effect.Has<EffectProcessedTag>())
                {
                    Apply(effect);
                }
            }
        }

        private static void Apply(SW.Entity effect)
        {
            ref readonly var targetRef = ref effect.Read<EffectTarget>();
            ref readonly var valueRef = ref effect.Read<EffectValue>();
            ref readonly var damageRef = ref effect.Read<DamageData>();

            if (!targetRef.Value.TryUnpack<ServerWT>(out var target))
            {
                effect.Set<EffectProcessedTag>();
                GetOrCreateDebugLogBuffer().Append(
                    $"discard damage target={targetRef.Value} type={damageRef.Type} reason=missing_target");
                return;
            }

            if (!target.Has<Health>())
                throw new InvalidOperationException(
                    $"DamageEffect target {target.GID} is missing required Health component.");

            ref var health = ref ReplicationMut.Mut<Health>(target);
            var damage = valueRef.Value < 0f ? 0f : valueRef.Value;
            var max = health.Max < 0f ? 0f : health.Max;
            var next = health.Current - damage;
            if (next < 0f)
                next = 0f;
            else if (next > max)
                next = max;

            var previous = health.Current;
            health.Current = next;
            effect.Set<EffectProcessedTag>();

            GetOrCreateDebugLogBuffer().Append(
                $"apply damage target={target.GID} type={damageRef.Type} previous={previous} current={health.Current}");
        }

        private static CombatDebugLogBuffer GetOrCreateDebugLogBuffer()
        {
            if (!SW.HasResource<CombatDebugLogBuffer>())
            {
                SW.SetResource(new CombatDebugLogBuffer());
                return SW.GetResource<CombatDebugLogBuffer>();
            }

            var buffer = SW.GetResource<CombatDebugLogBuffer>();
            if (buffer == null)
                throw new InvalidOperationException("Combat debug log buffer resource exists but is null.");

            return buffer;
        }
    }
}
