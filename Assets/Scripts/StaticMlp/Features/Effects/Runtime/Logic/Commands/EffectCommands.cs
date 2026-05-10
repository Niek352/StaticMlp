using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;

namespace StaticMlp.Features.Effects
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
            effect.Set(new EffectCreatedTick { Tick = SW.GetResource<SimulationTime>().ServerTick });
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

            SW.GetResource<CombatDebugLogBuffer>().Append(
                $"create damage source={source} target={target} value={value} type={type} request={requestId}");

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
