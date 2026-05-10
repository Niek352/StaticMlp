using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public sealed class ServerSynergyTriggerSystem : ISystem
    {
        private readonly List<EntityGID> _effects = new();
        private readonly List<EntityGID> _consumeOiledStatuses = new();

        public void Update()
        {
            _effects.Clear();
            _consumeOiledStatuses.Clear();
            foreach (var effect in SW.Query<All<EffectTag, DamageEffectTag, EffectTarget, EffectRequestId, DamageData, EffectChainData>, None<EffectProcessedTag, EffectRejectedTag>>().Entities())
                _effects.Add(effect.GID);

            var config = SW.GetResource<StatusesConfig>();
            for (var i = 0; i < _effects.Count; i++)
            {
                if (!_effects[i].TryUnpack<ServerWT>(out var effect))
                    continue;

                TryTrigger(effect, config);
            }

            for (var i = 0; i < _consumeOiledStatuses.Count; i++)
            {
                if (_consumeOiledStatuses[i].TryUnpack<ServerWT>(out var statusEntity))
                    statusEntity.Set<IsDestroyed>();
            }
        }

        private void TryTrigger(SW.Entity effect, StatusesConfig config)
        {
            ref readonly var damage = ref effect.Read<DamageData>();
            if (damage.Type != DamageType.Fire)
                return;

            ref readonly var targetRef = ref effect.Read<EffectTarget>();
            if (!targetRef.Value.TryUnpack<ServerWT>(out var target)
                || !target.Has<CharacterNetState>()
                || !StatusEntityLookup.TryFind<OiledStatus>(target.GID, out var oiledStatus))
            {
                return;
            }

            ref readonly var sourceRef = ref effect.Read<EffectSource>();
            ref readonly var requestRef = ref effect.Read<EffectRequestId>();
            ref readonly var chain = ref effect.Read<EffectChainData>();
            var nextDepth = (byte)(chain.Depth + 1);

            StatusEffectCommands.CreateBurningStatus(
                sourceRef.Value,
                target.GID,
                config.BurningDuration,
                config.BurningTickInterval,
                config.BurningPower,
                1,
                requestRef.Value,
                chain.RootEffectId,
                nextDepth,
                chain.MaxDepth);

            AreaEffectCommands.CreateBurningPool(
                sourceRef.Value,
                target.Read<CharacterNetState>().Position,
                config,
                requestRef.Value,
                chain.RootEffectId,
                nextDepth,
                chain.MaxDepth);

            _consumeOiledStatuses.Add(oiledStatus.GID);
        }
    }
}
