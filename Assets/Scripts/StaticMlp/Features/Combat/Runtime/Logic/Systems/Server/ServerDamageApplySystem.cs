using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Features.Shared;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerDamageApplySystem : ISystem
    {
        private readonly List<EntityGID> _pendingEffects = new();

        public void Update()
        {
            _pendingEffects.Clear();

            foreach (var effect in SW.Query<All<EffectTag, DamageEffectTag, EffectSource, EffectTarget, EffectValue, DamageData, EffectRequestId>, None<EffectProcessedTag, EffectRejectedTag>>().Entities())
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
            ref readonly var sourceRef = ref effect.Read<EffectSource>();
            ref readonly var valueRef = ref effect.Read<EffectValue>();
            ref readonly var damageRef = ref effect.Read<DamageData>();
            ref readonly var requestRef = ref effect.Read<EffectRequestId>();

            if (!targetRef.Value.TryUnpack<ServerWT>(out var target))
            {
                effect.Set<EffectProcessedTag>();
                SW.GetResource<CombatDebugLogBuffer>().Append(
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

            SW.GetResource<CombatDebugLogBuffer>().Append(
                $"apply damage target={target.GID} type={damageRef.Type} previous={previous} current={health.Current}");

            TrySendDamageNumber(sourceRef.Value, target.GID, requestRef.Value, damage, damageRef.Type);
            if (previous > 0f && health.Current <= 0f)
                TrySendDeathEvent(sourceRef.Value, target.GID, requestRef.Value);
        }

        private static void TrySendDamageNumber(EntityGID sourceGid, EntityGID targetGid, uint requestId, float damage, DamageType damageType)
        {
            if (requestId == 0
                || !sourceGid.TryUnpack<ServerWT>(out var source)
                || !source.Has<NetworkIdentity>())
            {
                return;
            }

            var evt = new DamageNumberEvent
            {
                Source = sourceGid,
                Target = targetGid,
                ClientCommandId = requestId,
                Value = damage,
                DamageType = damageType,
            };
            SW.SendToPeerEvent(source.Read<NetworkIdentity>().Owner, in evt);
        }

        private static void TrySendDeathEvent(EntityGID sourceGid, EntityGID targetGid, uint requestId)
        {
            if (requestId == 0
                || !sourceGid.TryUnpack<ServerWT>(out var source)
                || !source.Has<NetworkIdentity>())
            {
                return;
            }

            var evt = new DeathEvent
            {
                Source = sourceGid,
                Target = targetGid,
                ClientCommandId = requestId,
            };
            SW.SendToPeerEvent(source.Read<NetworkIdentity>().Owner, in evt);
        }
    }
}
