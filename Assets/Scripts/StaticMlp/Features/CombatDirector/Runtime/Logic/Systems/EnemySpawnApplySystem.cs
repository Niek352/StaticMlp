using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;
using UnityEngine;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EnemySpawnApplySystem : ISystem
    {
        private const float DEFAULT_ENEMY_MAX_HEALTH = 100f;
        private readonly List<EntityGID> _requests = new();

        public void Update()
        {
            _requests.Clear();
            foreach (var request in SW.Query<All<SpawnRequest, ValidSpawnRequestTag>>().Entities())
                _requests.Add(request.GID);

            if (_requests.Count == 0)
                return;

            var catalog = SW.GetResource<EnemySpawnCatalog>();
            var factory = SW.GetResource<AiBotFactory>();
            for (var i = 0; i < _requests.Count; i++)
            {
                if (!_requests[i].TryUnpack<ServerWT>(out var request))
                    continue;

                Apply(request, catalog, factory);
            }
        }

        private static void Apply(SW.Entity request, EnemySpawnCatalog catalog, AiBotFactory factory)
        {
            ref readonly var spawnRequest = ref request.Read<SpawnRequest>();
            var definition = catalog.Get(spawnRequest.Role);
            var directorEntity = ReadDirectorEntity(spawnRequest.CellId);
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            var initialTarget = SelectInitialTarget(in cell, spawnRequest.SpawnPosition);
            ref var budget = ref directorEntity.Mut<ThreatBudget>();
            var totalCost = definition.BudgetCost * spawnRequest.Count;
            if (budget.Current < totalCost)
                throw new InvalidOperationException("Threat budget became insufficient after spawn request validation.");

            for (var i = 0; i < spawnRequest.Count; i++)
            {
                var spawnedGid = factory.Spawn(new AiBotSpawnSpec(
                    AiBotsGameplayFeature.BOT,
                    ToVector3(spawnRequest.SpawnPosition),
                    Quaternion.identity,
                    CombatEnemyBehaviorIds.Default,
                    DEFAULT_ENEMY_MAX_HEALTH,
                    health01: 1f,
                    hunger: 0f,
                    fear: 0f,
                    leader: default,
                    initialEnemy: initialTarget));

                if (!spawnedGid.TryUnpack<ServerWT>(out var spawnedEntity))
                    throw new InvalidOperationException("Spawned enemy could not be unpacked in server world.");

                spawnedEntity.Set<EnemyTag>();
                spawnedEntity.Set(new EnemyArchetype
                {
                    Role = spawnRequest.Role
                });
                spawnedEntity.Set(new EnemySpawnSource
                {
                    SourceEntity = spawnRequest.SourceEntity,
                    SourceType = spawnRequest.SourceType,
                    SpawnPosition = spawnRequest.SpawnPosition
                });
                SW.SendEvent(new EnemySpawnedEvent(spawnedGid, spawnRequest.Role, spawnRequest.SourceType));
            }

            budget.Current -= totalCost;
            request.Destroy();
        }

        private static EntityGID SelectInitialTarget(in CombatCell cell, float3 spawnPosition)
        {
            var found = false;
            var bestDistanceSq = float.MaxValue;
            EntityGID target = default;

            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState>>().Entities())
            {
                var position = player.Read<CharacterNetState>().Position;
                if (math.distancesq(position, cell.Center) > cell.Radius * cell.Radius)
                    continue;

                var distanceSq = math.distancesq(position, spawnPosition);
                if (found && distanceSq >= bestDistanceSq)
                    continue;

                target = player.GID;
                bestDistanceSq = distanceSq;
                found = true;
            }

            return target;
        }

        private static SW.Entity ReadDirectorEntity(int cellId)
        {
            var found = false;
            SW.Entity directorEntity = default;

            foreach (var entity in SW.Query<All<CombatCell, ThreatBudget, DirectorState>>().Entities())
            {
                if (entity.Read<CombatCell>().CellId != cellId)
                    continue;

                if (found)
                    throw new InvalidOperationException("Combat Director found multiple director cells with the same cell id.");

                directorEntity = entity;
                found = true;
            }

            if (!found)
                throw new InvalidOperationException("Spawn request target combat cell must exist before enemy spawn apply.");

            return directorEntity;
        }

        private static Vector3 ToVector3(float3 value)
        {
            if (!math.all(math.isfinite(value)))
                throw new InvalidOperationException("Spawn position must be finite.");

            return new Vector3(value.x, value.y, value.z);
        }
    }
}
