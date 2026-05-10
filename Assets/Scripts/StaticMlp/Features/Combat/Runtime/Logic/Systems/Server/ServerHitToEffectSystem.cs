using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerHitToEffectSystem : ISystem
    {
        private readonly List<EntityGID> _hits = new();

        public void Update()
        {
            _hits.Clear();
            foreach (var hit in SW.Query<All<CombatHit>>().Entities())
                _hits.Add(hit.GID);

            var combatConfig = SW.GetResource<CombatConfig>();
            var statusesConfig = SW.GetResource<StatusesConfig>();
            for (var i = 0; i < _hits.Count; i++)
            {
                if (!_hits[i].TryUnpack<ServerWT>(out var hit))
                    continue;

                ApplyHit(hit, combatConfig, statusesConfig);
            }
        }

        private static void ApplyHit(SW.Entity hit, CombatConfig combatConfig, StatusesConfig statusesConfig)
        {
            ref readonly var data = ref hit.Read<CombatHit>();

            switch (data.AbilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    EffectCommands.CreateDamage(
                        data.Source,
                        data.Target,
                        combatConfig.PoisonArrowDamage,
                        DamageType.Physical,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        maxDepth: combatConfig.MaxChainDepth);
                    StatusEffectCommands.CreatePoisonStatus(
                        data.Source,
                        data.Target,
                        statusesConfig.PoisonDuration,
                        statusesConfig.PoisonTickInterval,
                        statusesConfig.PoisonPower,
                        1,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        depth: 1,
                        maxDepth: combatConfig.MaxChainDepth);
                    break;

                case CombatAbilityId.FireFlask:
                    EffectCommands.CreateDamage(
                        data.Source,
                        data.Target,
                        combatConfig.FireFlaskDamage,
                        DamageType.Fire,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        maxDepth: combatConfig.MaxChainDepth);
                    break;

                default:
                    EffectCommands.CreateDamage(
                        data.Source,
                        data.Target,
                        combatConfig.DamageValue,
                        DamageType.Physical,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        maxDepth: combatConfig.MaxChainDepth);
                    break;
            }

            hit.Destroy();
        }
    }
}
