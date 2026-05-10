using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public sealed class ServerPoisonStatusTickSystem : ISystem
    {
        public void Update()
        {
            var currentTick = SW.GetResource<SimulationTime>().ServerTick;

            foreach (var statusEntity in SW.Query<All<PoisonStatus, LifeTime, StatusTarget, StatusStrength, StatusTickState, StatusContext>, None<IsDestroyed>>().Entities())
            {
                ref readonly var lifeTime = ref statusEntity.Read<LifeTime>();
                if (currentTick >= lifeTime.EndTick)
                    continue;

                var target = statusEntity.Read<StatusTarget>().Value;
                if (!target.TryUnpack<ServerWT>(out _))
                {
                    statusEntity.Set<IsDestroyed>();
                    continue;
                }

                ref var tick = ref statusEntity.Mut<StatusTickState>();
                if (tick.IntervalTicks == 0)
                    continue;

                ref readonly var strength = ref statusEntity.Read<StatusStrength>();
                ref readonly var context = ref statusEntity.Read<StatusContext>();
                while (currentTick >= tick.NextTick && currentTick < lifeTime.EndTick)
                {
                    tick.NextTick += tick.IntervalTicks;
                    EffectCommands.CreateDamage(
                        context.Source,
                        target,
                        strength.Power * Math.Max((byte)1, strength.Stacks),
                        DamageType.Poison,
                        context.RequestId,
                        context.RootEffectId,
                        (byte)(context.ChainDepth + 1),
                        context.MaxDepth);
                }
            }
        }
    }
}
