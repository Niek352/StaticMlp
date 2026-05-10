using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Statuses
{
    public static class StatusEffectCommands
    {
        public static SW.Entity CreatePoisonStatus(
            EntityGID source,
            EntityGID target,
            float duration,
            float tickInterval,
            float power,
            byte stacks,
            uint requestId = 0,
            uint rootEffectId = 0,
            byte depth = 0,
            byte maxDepth = 4)
        {
            return CreateStatusEffect<PoisonStatus>(
                source,
                target,
                duration,
                tickInterval,
                power,
                stacks,
                requestId,
                rootEffectId,
                depth,
                maxDepth,
                "poison");
        }

        public static SW.Entity CreateBurningStatus(
            EntityGID source,
            EntityGID target,
            float duration,
            float tickInterval,
            float power,
            byte stacks,
            uint requestId = 0,
            uint rootEffectId = 0,
            byte depth = 0,
            byte maxDepth = 4)
        {
            return CreateStatusEffect<BurningStatus>(
                source,
                target,
                duration,
                tickInterval,
                power,
                stacks,
                requestId,
                rootEffectId,
                depth,
                maxDepth,
                "burning");
        }

        public static SW.Entity CreateOiledStatus(
            EntityGID source,
            EntityGID target,
            float duration,
            float tickInterval,
            float power,
            byte stacks,
            uint requestId = 0,
            uint rootEffectId = 0,
            byte depth = 0,
            byte maxDepth = 4)
        {
            return CreateStatusEffect<OiledStatus>(
                source,
                target,
                duration,
                tickInterval,
                power,
                stacks,
                requestId,
                rootEffectId,
                depth,
                maxDepth,
                "oiled");
        }

        private static SW.Entity CreateStatusEffect<TStatusTag>(
            EntityGID source,
            EntityGID target,
            float duration,
            float tickInterval,
            float power,
            byte stacks,
            uint requestId,
            uint rootEffectId,
            byte depth,
            byte maxDepth,
            string statusName)
            where TStatusTag : struct, ITag
        {
            var effect = SW.NewEntity<EffectEntityType>();
            effect.Set<EffectTag>();
            effect.Set<AddStatusEffectTag>();
            effect.Set<TStatusTag>();
            effect.Set(new EffectKind { Value = EffectType.AddStatus });
            effect.Set(new EffectSource { Value = source });
            effect.Set(new EffectTarget { Value = target });
            effect.Set(new EffectValue { Value = power });
            effect.Set(new EffectCreatedTick { Tick = (uint)Time.frameCount });
            effect.Set(new EffectRequestId { Value = requestId });
            effect.Set(new AddStatusSpec
            {
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

            SW.GetResource<CombatDebugLogBuffer>().Append(
                $"create status source={source} target={target} type={statusName} duration={duration} request={requestId}");

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
