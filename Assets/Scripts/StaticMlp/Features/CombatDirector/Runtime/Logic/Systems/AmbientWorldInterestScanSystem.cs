using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class AmbientWorldInterestScanSystem : ISystem
    {
        public void Update()
        {
            var config = SW.GetResource<EncounterDirectorConfig>();
            ValidateConfig(config);
            TickMarkerCooldowns();

            var directorEntity = ReadDirectorEntity();
            ref readonly var state = ref directorEntity.Read<DirectorState>();
            if (!IsAmbientSpawnPhase(state.Phase) || directorEntity.Has<EncounterState>() || HasPendingSpawnRequest())
            {
                ClearAmbientSelection(directorEntity);
                UpdateCaps(directorEntity, config);
                return;
            }

            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            var ambientAliveCount = CountAliveEnemies(in cell);
            UpdateCaps(directorEntity, config, ambientAliveCount);
            if (ambientAliveCount >= config.AmbientMaxAliveEnemiesPerCell)
            {
                ClearAmbientSelection(directorEntity);
                return;
            }

            if (!TrySelectAmbientSource(in cell, config, out var selection))
            {
                ClearAmbientSelection(directorEntity);
                return;
            }

            directorEntity.Set(selection);
        }

        private static void ValidateConfig(EncounterDirectorConfig config)
        {
            if (config.AmbientMinAliveEnemiesPerCell < 1)
                throw new InvalidOperationException("EncounterDirectorConfig.AmbientMinAliveEnemiesPerCell must be at least 1.");

            if (config.AmbientMaxAliveEnemiesPerCell < config.AmbientMinAliveEnemiesPerCell)
                throw new InvalidOperationException("EncounterDirectorConfig.AmbientMaxAliveEnemiesPerCell must be at least the ambient minimum.");

            if (config.AmbientMaxAliveEnemiesPerCell > 4)
                throw new InvalidOperationException("EncounterDirectorConfig.AmbientMaxAliveEnemiesPerCell must stay within the early ambient cap of 4.");

            if (config.EncounterMinAliveEnemiesPerCell < 1 || config.EncounterMaxAliveEnemiesPerCell < config.EncounterMinAliveEnemiesPerCell)
                throw new InvalidOperationException("EncounterDirectorConfig encounter alive caps are invalid.");

            if (config.EscalationMinAliveEnemiesPerCell < 2 || config.EscalationMaxAliveEnemiesPerCell < config.EscalationMinAliveEnemiesPerCell)
                throw new InvalidOperationException("EncounterDirectorConfig escalation alive caps are invalid.");

            if (config.AmbientMaxEnemiesPerRequest < 1 || config.AmbientMaxEnemiesPerRequest > config.AmbientMaxAliveEnemiesPerCell)
                throw new InvalidOperationException("EncounterDirectorConfig.AmbientMaxEnemiesPerRequest must fit the ambient cell cap.");

            if (config.AmbientSpawnCooldownSeconds <= 0f)
                throw new InvalidOperationException("EncounterDirectorConfig.AmbientSpawnCooldownSeconds must be positive.");
        }

        private static void TickMarkerCooldowns()
        {
            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            foreach (var source in SW.Query<All<AmbientSpawnMarker>>().Entities())
            {
                ref var marker = ref source.Mut<AmbientSpawnMarker>();
                if (marker.CooldownRemaining <= 0f)
                    continue;

                marker.CooldownRemaining = math.max(0f, marker.CooldownRemaining - deltaTime);
            }
        }

        private static bool TrySelectAmbientSource(
            in CombatCell cell,
            EncounterDirectorConfig config,
            out SelectedSpawnSource selection)
        {
            var found = false;
            var bestDistanceScore = float.MaxValue;
            selection = default;

            foreach (var entity in SW.Query<All<SpawnSource>>().Entities())
            {
                ref readonly var source = ref entity.Read<SpawnSource>();
                if (!source.IsActive || !source.AllowsAmbient)
                    continue;

                if (!math.all(math.isfinite(source.Position)))
                    throw new InvalidOperationException("Ambient spawn source position must be finite.");

                if (!TryReadAmbientMarker(entity, config, out var marker))
                    continue;

                var distance = math.distance(cell.Center, source.Position);
                if (distance < config.MinSpawnSourceDistance || distance > config.MaxSpawnSourceDistance)
                    continue;

                var distanceScore = math.abs(distance - cell.Radius);
                if (found && distanceScore >= bestDistanceScore)
                    continue;

                selection = new SelectedSpawnSource
                {
                    CellId = cell.CellId,
                    SourceEntity = entity.GID,
                    SourceType = source.Type,
                    SourceKind = source.Kind,
                    AmbientKind = marker.Kind,
                    AmbientMinCount = marker.MinCount,
                    AmbientMaxCount = marker.MaxCount,
                    Position = source.Position
                };
                bestDistanceScore = distanceScore;
                found = true;
            }

            return found;
        }

        private static bool TryReadAmbientMarker(
            SW.Entity sourceEntity,
            EncounterDirectorConfig config,
            out AmbientSpawnMarker marker)
        {
            if (sourceEntity.Has<AmbientSpawnMarker>())
            {
                marker = sourceEntity.Read<AmbientSpawnMarker>();
                if (marker.CooldownRemaining > 0f)
                    return false;

                ValidateMarker(in marker);
                return true;
            }

            marker = new AmbientSpawnMarker
            {
                Kind = AmbientSpawnKind.SoloAnimal,
                MinCount = 1,
                MaxCount = 1,
                CooldownSeconds = config.AmbientSpawnCooldownSeconds,
                CooldownRemaining = 0f
            };
            return true;
        }

        private static void ValidateMarker(in AmbientSpawnMarker marker)
        {
            if (marker.Kind == AmbientSpawnKind.None)
                throw new InvalidOperationException("Ambient spawn marker kind must be explicit.");

            if (marker.MinCount < 1 || marker.MaxCount < marker.MinCount || marker.MaxCount > 4)
                throw new InvalidOperationException("Ambient spawn marker counts must stay within 1..4.");

            if (marker.CooldownSeconds <= 0f)
                throw new InvalidOperationException("Ambient spawn marker cooldown must be positive.");
        }

        private static SW.Entity ReadDirectorEntity()
        {
            var found = false;
            SW.Entity directorEntity = default;

            foreach (var entity in SW.Query<All<CombatCell, CellAliveEnemyCaps, DirectorState>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Combat Director currently supports only a single director cell.");

                directorEntity = entity;
                found = true;
            }

            if (!found)
                throw new InvalidOperationException("Combat Director cell entity must exist before ambient scanning.");

            return directorEntity;
        }

        private static bool IsAmbientSpawnPhase(DirectorPhase phase)
        {
            return phase == DirectorPhase.Ambient || phase == DirectorPhase.Contact;
        }

        private static bool HasPendingSpawnRequest()
        {
            foreach (var _ in SW.Query<All<SpawnRequest>>().Entities())
                return true;

            return false;
        }

        private static void UpdateCaps(SW.Entity directorEntity, EncounterDirectorConfig config, int aliveEnemyCount = 0)
        {
            ref var caps = ref directorEntity.Mut<CellAliveEnemyCaps>();
            caps.AmbientMinAliveEnemies = config.AmbientMinAliveEnemiesPerCell;
            caps.AmbientMaxAliveEnemies = config.AmbientMaxAliveEnemiesPerCell;
            caps.EncounterMinAliveEnemies = config.EncounterMinAliveEnemiesPerCell;
            caps.EncounterMaxAliveEnemies = config.EncounterMaxAliveEnemiesPerCell;
            caps.EscalationMinAliveEnemies = config.EscalationMinAliveEnemiesPerCell;
            caps.EscalationMaxAliveEnemies = config.EscalationMaxAliveEnemiesPerCell;
            caps.AmbientAliveEnemies = aliveEnemyCount;
            caps.EncounterAliveEnemies = aliveEnemyCount;
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

        private static void ClearAmbientSelection(SW.Entity directorEntity)
        {
            if (directorEntity.Has<SelectedSpawnSource>()
                && directorEntity.Read<SelectedSpawnSource>().AmbientKind != AmbientSpawnKind.None)
            {
                directorEntity.Delete<SelectedSpawnSource>();
            }
        }
    }
}
