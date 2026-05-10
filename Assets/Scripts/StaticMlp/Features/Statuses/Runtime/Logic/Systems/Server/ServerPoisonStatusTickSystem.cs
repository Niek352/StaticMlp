using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Statuses
{
    public sealed class ServerPoisonStatusTickSystem : ISystem
    {
        private readonly Func<float> _deltaTimeProvider;

        public ServerPoisonStatusTickSystem(Func<float> deltaTimeProvider = null)
        {
            _deltaTimeProvider = deltaTimeProvider ?? (() => Time.deltaTime);
        }

        public void Update()
        {
            var deltaTime = Mathf.Max(0f, _deltaTimeProvider());

            foreach (var statusEntity in SW.Query<All<PoisonStatus, LifeTime, StatusTarget, StatusStrength, StatusTickState, StatusContext>, None<IsDestroyed>>().Entities())
            {
                ref var lifeTime = ref statusEntity.Mut<LifeTime>();
                lifeTime.RemainingTime -= deltaTime;
                if (lifeTime.RemainingTime <= 0f)
                    continue;

                var target = statusEntity.Read<StatusTarget>().Value;
                if (!target.TryUnpack<ServerWT>(out _))
                {
                    statusEntity.Set<IsDestroyed>();
                    continue;
                }

                ref var tick = ref statusEntity.Mut<StatusTickState>();
                tick.Timer += deltaTime;
                if (tick.Interval <= 0f)
                    continue;

                ref readonly var strength = ref statusEntity.Read<StatusStrength>();
                ref readonly var context = ref statusEntity.Read<StatusContext>();
                while (tick.Timer >= tick.Interval)
                {
                    tick.Timer -= tick.Interval;
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
