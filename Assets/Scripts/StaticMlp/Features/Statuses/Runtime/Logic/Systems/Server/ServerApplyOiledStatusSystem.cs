using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Statuses
{
    public sealed class ServerApplyOiledStatusSystem : ISystem
    {
        private readonly List<EntityGID> _effects = new();

        public void Update()
        {
            _effects.Clear();
            foreach (var effect in SW.Query<All<EffectTag, AddStatusEffectTag, OiledStatus, EffectSource, EffectTarget, EffectRequestId, AddStatusSpec, EffectChainData>, None<EffectProcessedTag, EffectRejectedTag>>().Entities())
                _effects.Add(effect.GID);

            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].TryUnpack<ServerWT>(out var effect))
                    Apply(effect);
            }
        }

        private static void Apply(SW.Entity effect)
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            var targetGid = effect.Read<EffectTarget>().Value;
            if (!targetGid.TryUnpack<ServerWT>(out var target))
            {
                effect.Set<EffectProcessedTag>();
                return;
            }

            ref readonly var source = ref effect.Read<EffectSource>();
            ref readonly var spec = ref effect.Read<AddStatusSpec>();
            ref readonly var request = ref effect.Read<EffectRequestId>();
            ref readonly var chain = ref effect.Read<EffectChainData>();

            if (StatusEntityLookup.TryFind<OiledStatus>(target.GID, out var statusEntity))
            {
                ref var lifeTime = ref statusEntity.Mut<LifeTime>();
                lifeTime.EndTick = Math.Max(lifeTime.EndTick, simulationTime.DeadlineAfter(spec.Duration));

                ref var strength = ref ReplicationMut.Mut<StatusStrength>(statusEntity);
                strength.Power = Math.Max(strength.Power, spec.Power);
                strength.Stacks = (byte)Math.Min(byte.MaxValue, strength.Stacks + Math.Max((byte)1, spec.Stacks));

                ref var context = ref ReplicationMut.Mut<StatusContext>(statusEntity);
                context.Source = source.Value;
                context.RequestId = request.Value;
                context.RootEffectId = chain.RootEffectId;
                context.ChainDepth = chain.Depth;
                context.MaxDepth = chain.MaxDepth;
            }
            else
            {
                StatusEntitySpawns.SpawnOiled(target, source.Value, spec, request.Value, chain, simulationTime);
            }

            effect.Set<EffectProcessedTag>();
        }
    }
}
