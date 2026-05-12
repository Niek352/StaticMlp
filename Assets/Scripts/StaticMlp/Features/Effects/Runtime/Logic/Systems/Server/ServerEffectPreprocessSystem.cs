using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Effects
{
    public sealed class ServerEffectPreprocessSystem : ISystem
    {
        private readonly List<EntityGID> _effects = new();

        public void Update()
        {
            _effects.Clear();
            foreach (var effect in SW.Query<All<EffectTag, EffectTarget, EffectChainData>, None<EffectProcessedTag, EffectRejectedTag>>().Entities())
                _effects.Add(effect.GID);

            for (var i = 0; i < _effects.Count; i++)
            {
                if (!_effects[i].TryUnpack<ServerWT>(out var effect))
                    continue;

                Preprocess(effect);
            }
        }

        private static void Preprocess(SW.Entity effect)
        {
            ref readonly var target = ref effect.Read<EffectTarget>();
            ref readonly var chain = ref effect.Read<EffectChainData>();

            if (!target.Value.TryUnpack<ServerWT>(out _))
            {
                Reject(effect, EffectRejectedReasonCode.MissingTarget, $"reject effect target={target.Value} reason=missing_target");
                return;
            }

            if (chain.Depth > chain.MaxDepth)
                Reject(effect, EffectRejectedReasonCode.ChainDepthExceeded, $"reject effect root={chain.RootEffectId} reason=chain_depth");
        }

        private static void Reject(SW.Entity effect, EffectRejectedReasonCode reason, string log)
        {
            EffectLifecycle.MarkRejected(effect);
            effect.Set(new EffectRejectedReason { Value = reason });
            SW.GetResource<CombatDebugLogBuffer>().Append(log);
        }
    }
}
