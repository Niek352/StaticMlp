using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class SpawnRequestValidationSystem : ISystem
    {
        private readonly List<EntityGID> _requests = new();

        public void Update()
        {
            _requests.Clear();
            foreach (var request in SW.Query<All<SpawnRequest>, None<ValidSpawnRequestTag>>().Entities())
                _requests.Add(request.GID);

            if (_requests.Count == 0)
                return;

            var config = SW.GetResource<EncounterDirectorConfig>();
            var catalog = SW.GetResource<EnemySpawnCatalog>();
            var directorEntity = ReadDirectorEntity();
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            ref readonly var state = ref directorEntity.Read<DirectorState>();
            ref readonly var budget = ref directorEntity.Read<ThreatBudget>();
            var acceptedCount = 0;
            var acceptedBudgetCost = 0f;
            var aliveCount = CountAliveEnemies(in cell);

            for (var i = 0; i < _requests.Count; i++)
            {
                if (!_requests[i].TryUnpack<ServerWT>(out var request))
                    continue;

                ValidateRequest(
                    request,
                    in cell,
                    in state,
                    budget.Current,
                    config,
                    catalog,
                    aliveCount,
                    ref acceptedCount,
                    ref acceptedBudgetCost);
            }
        }

        private static void ValidateRequest(
            SW.Entity request,
            in CombatCell cell,
            in DirectorState state,
            float currentBudget,
            EncounterDirectorConfig config,
            EnemySpawnCatalog catalog,
            int aliveCount,
            ref int acceptedCount,
            ref float acceptedBudgetCost)
        {
            ref readonly var spawnRequest = ref request.Read<SpawnRequest>();

            if (!IsSpawnPhase(state.Phase)
                || spawnRequest.CellId != cell.CellId
                || spawnRequest.Count <= 0
                || !math.all(math.isfinite(spawnRequest.SpawnPosition)))
            {
                request.Destroy();
                return;
            }

            if (!catalog.TryGet(spawnRequest.Role, out var definition))
            {
                request.Destroy();
                return;
            }

            if (!IsSourceStillValid(in spawnRequest, in cell, config))
            {
                request.Destroy();
                return;
            }

            var totalAfterSpawn = aliveCount + acceptedCount + spawnRequest.Count;
            if (totalAfterSpawn > config.MaxAliveEnemiesPerCell)
            {
                request.Destroy();
                return;
            }

            var requestCost = spawnRequest.Count * definition.BudgetCost;
            if (acceptedBudgetCost + requestCost > currentBudget)
            {
                request.Destroy();
                return;
            }

            acceptedCount += spawnRequest.Count;
            acceptedBudgetCost += requestCost;
            request.Set<ValidSpawnRequestTag>();
        }

        private static SW.Entity ReadDirectorEntity()
        {
            var found = false;
            SW.Entity directorEntity = default;

            foreach (var entity in SW.Query<All<CombatCell, ThreatBudget, DirectorState>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Combat Director currently supports only a single director cell.");

                directorEntity = entity;
                found = true;
            }

            if (!found)
                throw new InvalidOperationException("Combat Director cell entity must exist before spawn request validation.");

            return directorEntity;
        }

        private static bool IsSourceStillValid(
            in SpawnRequest request,
            in CombatCell cell,
            EncounterDirectorConfig config)
        {
            if (!request.SourceEntity.TryUnpack<ServerWT>(out var sourceEntity) || !sourceEntity.Has<SpawnSource>())
                return false;

            ref readonly var source = ref sourceEntity.Read<SpawnSource>();
            if (!source.IsActive || source.Type != request.SourceType)
                return false;

            if (math.distancesq(source.Position, request.SpawnPosition) > 0.0001f)
                return false;

            var distance = math.distance(cell.Center, source.Position);
            return distance >= config.MinSpawnSourceDistance && distance <= config.MaxSpawnSourceDistance;
        }

        private static bool IsSpawnPhase(DirectorPhase phase)
        {
            return phase == DirectorPhase.BuildUp || phase == DirectorPhase.Peak;
        }

        private static int CountAliveEnemies(in CombatCell cell)
        {
            var aliveEnemyCount = 0;

            foreach (var entity in SW.Query<All<EnemyTag, CharacterNetState>, None<IsDiedTag>>().Entities())
            {
                var position = ToFloat3(entity.Read<CharacterNetState>().Position);
                if (!math.all(math.isfinite(position)))
                    throw new InvalidOperationException("Enemy position must be finite.");

                if (math.distancesq(position, cell.Center) > cell.Radius * cell.Radius)
                    continue;

                aliveEnemyCount++;
            }

            return aliveEnemyCount;
        }

        private static float3 ToFloat3(UnityEngine.Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }
    }
}
