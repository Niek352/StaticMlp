using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerStatusTickSystem : ISystem
    {
        private readonly Func<float> _deltaTimeProvider;

        public ServerStatusTickSystem(Func<float> deltaTimeProvider = null)
        {
            _deltaTimeProvider = deltaTimeProvider ?? (() => Time.deltaTime);
        }

        public void Update()
        {
            var deltaTime = Mathf.Max(0f, _deltaTimeProvider());

            TickPoison(deltaTime);
            TickBurning(deltaTime);
        }

        private static void TickPoison(float deltaTime)
        {
            foreach (var entity in SW.Query<All<PoisonStatus>>().Entities())
            {
                ref var status = ref ReplicationMut.Mut<PoisonStatus>(entity);
                status.RemainingTime -= deltaTime;
                status.TickTimer += deltaTime;
                if (status.TickInterval <= 0f)
                    continue;

                while (status.TickTimer >= status.TickInterval)
                {
                    status.TickTimer -= status.TickInterval;
                    EffectCommands.CreateDamage(
                        status.Source,
                        entity.GID,
                        status.Power * Math.Max((byte)1, status.Stacks),
                        DamageType.Poison,
                        status.RequestId,
                        status.RootEffectId,
                        (byte)(status.ChainDepth + 1),
                        status.MaxDepth);
                }
            }
        }

        private static void TickBurning(float deltaTime)
        {
            foreach (var entity in SW.Query<All<BurningStatus>>().Entities())
            {
                ref var status = ref ReplicationMut.Mut<BurningStatus>(entity);
                status.RemainingTime -= deltaTime;
                status.TickTimer += deltaTime;
                if (status.TickInterval <= 0f)
                    continue;

                while (status.TickTimer >= status.TickInterval)
                {
                    status.TickTimer -= status.TickInterval;
                    EffectCommands.CreateDamage(
                        status.Source,
                        entity.GID,
                        status.Power * Math.Max((byte)1, status.Stacks),
                        DamageType.Fire,
                        status.RequestId,
                        status.RootEffectId,
                        (byte)(status.ChainDepth + 1),
                        status.MaxDepth);
                }
            }
        }
    }
}
