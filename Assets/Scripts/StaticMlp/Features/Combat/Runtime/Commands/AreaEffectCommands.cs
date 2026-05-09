using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public static class AreaEffectCommands
    {
        public static SW.Entity CreateBurningPool(
            EntityGID source,
            Vector3 position,
            CombatAutoAttackConfig config,
            uint requestId = 0,
            uint rootEffectId = 0,
            byte depth = 0,
            byte maxDepth = 4)
        {
            var area = SW.NewEntity<Default>();
            area.Set<AreaEffectTag>();
            area.Set(new AreaEffectState
            {
                Position = position,
                Radius = config.BurningPoolRadius,
                RemainingTime = config.BurningPoolDuration,
                TickInterval = config.BurningPoolTickInterval,
                TickTimer = 0f,
                DamagePerTick = config.BurningPoolDamage,
                Source = source,
                RequestId = requestId,
                RootEffectId = rootEffectId,
                ChainDepth = depth,
                MaxDepth = maxDepth,
                DamageType = DamageType.Fire,
            });

            CombatDebugLogBufferAccess.GetOrCreate().Append(
                $"create area type=burning_pool source={source} radius={config.BurningPoolRadius} request={requestId}");

            return area;
        }
    }
}
