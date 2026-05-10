using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Shared;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Statuses
{
    public sealed class ClientStatusAuraViewStateSystem : ISystem
    {
        private readonly Dictionary<EntityGID, StatusVisualFlags> _flagsByTarget = new();

        public void Update()
        {
            _flagsByTarget.Clear();
            foreach (var statusEntity in CW.Query<All<StatusTarget>>().Entities())
            {
                var flags = StatusVisualFlags.None;
                if (statusEntity.Has<PoisonStatus>())
                    flags |= StatusVisualFlags.Poison;
                if (statusEntity.Has<BurningStatus>())
                    flags |= StatusVisualFlags.Burning;
                if (statusEntity.Has<OiledStatus>())
                    flags |= StatusVisualFlags.Oiled;

                if (flags == StatusVisualFlags.None)
                    continue;

                var target = statusEntity.Read<StatusTarget>().Value;
                _flagsByTarget.TryGetValue(target, out var current);
                _flagsByTarget[target] = current | flags;
            }

            foreach (var entity in CW.Query<All<CharacterNetState>>().Entities())
            {
                var healthNormalized = 1f;
                var isDead = false;
                if (RequiresHealth(entity))
                {
                    if (!entity.Has<Health>())
                        throw new InvalidOperationException(
                            $"Client status aura actor {entity.GID} ({GetActorKind(entity)}) is missing replicated {nameof(Health)}.");

                    ref readonly var health = ref entity.Read<Health>();
                    isDead = health.Current <= 0f;
                    healthNormalized = health.Max > 0f ? Mathf.Clamp01(health.Current / health.Max) : 0f;
                }

                _flagsByTarget.TryGetValue(entity.GID, out var flags);

                entity.Set(new StatusAuraViewState
                {
                    Flags = flags,
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
