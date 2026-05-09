using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
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

            var config = SW.GetResource<CombatAutoAttackConfig>();
            for (var i = 0; i < _hits.Count; i++)
            {
                if (!_hits[i].TryUnpack<ServerWT>(out var hit))
                    continue;

                ApplyHit(hit, config);
            }
        }

        private static void ApplyHit(SW.Entity hit, CombatAutoAttackConfig config)
        {
            ref readonly var data = ref hit.Read<CombatHit>();

            switch (data.AbilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    EffectCommands.CreateDamage(
                        data.Source,
                        data.Target,
                        config.PoisonArrowDamage,
                        DamageType.Physical,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        maxDepth: config.MaxChainDepth);
                    EffectCommands.CreateAddStatus(
                        data.Source,
                        data.Target,
                        StatusType.Poison,
                        config.PoisonDuration,
                        config.PoisonTickInterval,
                        config.PoisonPower,
                        1,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        depth: 1,
                        maxDepth: config.MaxChainDepth);
                    break;

                case CombatAbilityId.FireFlask:
                    EffectCommands.CreateDamage(
                        data.Source,
                        data.Target,
                        config.FireFlaskDamage,
                        DamageType.Fire,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        maxDepth: config.MaxChainDepth);
                    break;

                default:
                    EffectCommands.CreateDamage(
                        data.Source,
                        data.Target,
                        config.DamageValue,
                        DamageType.Physical,
                        data.ClientCommandId,
                        rootEffectId: data.ClientCommandId,
                        maxDepth: config.MaxChainDepth);
                    break;
            }

            hit.Destroy();
        }
    }
}
