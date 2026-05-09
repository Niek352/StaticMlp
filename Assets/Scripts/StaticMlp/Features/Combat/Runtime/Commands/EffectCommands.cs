using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public static class EffectCommands
    {
        public static SW.Entity CreateDamage(
            EntityGID source,
            EntityGID target,
            float value,
            DamageType type,
            uint requestId = 0)
        {
            var effect = SW.NewEntity<EffectEntityType>();
            effect.Set<EffectTag>();
            effect.Set<DamageEffectTag>();
            effect.Set(new EffectKind { Value = EffectType.Damage });
            effect.Set(new EffectSource { Value = source });
            effect.Set(new EffectTarget { Value = target });
            effect.Set(new EffectValue { Value = value });
            effect.Set(new EffectCreatedTick { Tick = (uint)Time.frameCount });
            effect.Set(new EffectRequestId { Value = requestId });
            effect.Set(new DamageData { Type = type });

            CombatDebugLogBufferAccess.GetOrCreate().Append(
                $"create damage source={source} target={target} value={value} type={type} request={requestId}");

            return effect;
        }
    }
}
