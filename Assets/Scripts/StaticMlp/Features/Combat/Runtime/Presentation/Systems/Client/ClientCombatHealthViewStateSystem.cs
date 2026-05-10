using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Features.Shared;
using System;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientCombatHealthViewStateSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in CW.Query<All<CharacterNetState>>().Entities())
            {
                var healthNormalized = 1f;
                var isDead = false;
                if (RequiresHealth(entity))
                {
                    if (!entity.Has<Health>())
                        throw new InvalidOperationException(
                            $"Client combat actor {entity.GID} ({GetActorKind(entity)}) is missing replicated {nameof(Health)}. " +
                            "This actor must carry server-replicated Health on the client.");

                    ref readonly var health = ref entity.Read<Health>();
                    isDead = health.Current <= 0f;
                    healthNormalized = health.Max > 0f ? Mathf.Clamp01(health.Current / health.Max) : 0f;
                }

                entity.Set(new CombatHealthViewState
                {
                    HealthNormalized = healthNormalized,
                    IsDead = isDead,
                });
            }
        }

        private static bool RequiresHealth(CW.Entity entity)
        {
            return entity.Has<PlayerTag>() || entity.Has<MonsterTag>();
        }

        private static string GetActorKind(CW.Entity entity)
        {
            if (entity.Has<PlayerTag>())
                return nameof(PlayerTag);

            if (entity.Has<MonsterTag>())
                return nameof(MonsterTag);

            return nameof(CharacterNetState);
        }
    }
}
