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
            uint requestId = 0,
            uint rootEffectId = 0,
            byte depth = 0,
            byte maxDepth = 4)
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
            effect.Set(new DamageData
            {
                Type = type,
                ArmorPierce = 0f,
                CanCrit = false,
                CanTriggerOnHit = false
            });
            effect.Set(new EffectChainData
            {
                RootEffectId = ResolveRootEffectId(effect, requestId, rootEffectId),
                Depth = depth,
                MaxDepth = maxDepth,
            });

            CombatDebugLogBufferAccess.GetOrCreate().Append(
                $"create damage source={source} target={target} value={value} type={type} request={requestId}");

            return effect;
        }

        public static SW.Entity CreateAddStatus(
            EntityGID source,
            EntityGID target,
            StatusType statusType,
            float duration,
            float tickInterval,
            float power,
            byte stacks,
            uint requestId = 0,
            uint rootEffectId = 0,
            byte depth = 0,
            byte maxDepth = 4)
        {
            var effect = SW.NewEntity<EffectEntityType>();
            effect.Set<EffectTag>();
            effect.Set<AddStatusEffectTag>();
            effect.Set(new EffectKind { Value = EffectType.AddStatus });
            effect.Set(new EffectSource { Value = source });
            effect.Set(new EffectTarget { Value = target });
            effect.Set(new EffectValue { Value = power });
            effect.Set(new EffectCreatedTick { Tick = (uint)Time.frameCount });
            effect.Set(new EffectRequestId { Value = requestId });
            effect.Set(new AddStatusData
            {
                Type = statusType,
                Duration = duration,
                TickInterval = tickInterval,
                Power = power,
                Stacks = stacks,
            });
            effect.Set(new EffectChainData
            {
                RootEffectId = ResolveRootEffectId(effect, requestId, rootEffectId),
                Depth = depth,
                MaxDepth = maxDepth,
            });

            CombatDebugLogBufferAccess.GetOrCreate().Append(
                $"create status source={source} target={target} type={statusType} duration={duration} request={requestId}");

            return effect;
        }

        private static uint ResolveRootEffectId(SW.Entity effect, uint requestId, uint rootEffectId)
        {
            if (rootEffectId != 0)
                return rootEffectId;

            if (requestId != 0)
                return requestId;

            var raw = effect.GID.Raw;
            return (uint)(raw ^ (raw >> 32));
        }
    }
}
