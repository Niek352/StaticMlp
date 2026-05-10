using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Statuses
{
    public static class AreaEffectCommands
    {
        public static SW.Entity CreateBurningPool(
            EntityGID source,
            Vector3 position,
            StatusesConfig config,
            uint requestId = 0,
            uint rootEffectId = 0,
            byte depth = 0,
            byte maxDepth = 4)
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            var area = SW.NewEntity<Default>();
            area.Set<AreaEffectTag>();
            area.Set(new LifeTime
            {
                EndTick = simulationTime.DeadlineAfter(config.BurningPoolDuration),
            });
            area.Set(new AreaEffectState
            {
                Position = position,
                Radius = config.BurningPoolRadius,
                TickIntervalTicks = simulationTime.SecondsToTicks(config.BurningPoolTickInterval),
                NextDamageTick = simulationTime.DeadlineAfter(config.BurningPoolTickInterval),
                DamagePerTick = config.BurningPoolDamage,
                Source = source,
                RequestId = requestId,
                RootEffectId = rootEffectId,
                ChainDepth = depth,
                MaxDepth = maxDepth,
                DamageType = DamageType.Fire,
            });

            SW.GetResource<CombatDebugLogBuffer>().Append(
                $"create area type=burning_pool source={source} radius={config.BurningPoolRadius} request={requestId}");

            return area;
        }
    }
}
