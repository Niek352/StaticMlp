using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;
using UnityEngine;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class SpawnSourceSelectionSystem : ISystem
    {
        public void Update()
        {
            var config = SW.GetResource<EncounterDirectorConfig>();
            ValidateConfig(config);

            var directorEntity = ReadDirectorEntity();
            ref readonly var state = ref directorEntity.Read<DirectorState>();
            if (!IsSpawnPhase(state.Phase))
            {
                ClearSelection(directorEntity);
                return;
            }

            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            if (!TrySelectSource(in cell, config, out var selection))
            {
                ClearSelection(directorEntity);
                return;
            }

            directorEntity.Set(selection);
        }

        private static void ValidateConfig(EncounterDirectorConfig config)
        {
            if (config.MinSpawnSourceDistance < 0f)
                throw new InvalidOperationException("EncounterDirectorConfig.MinSpawnSourceDistance must be non-negative.");

            if (config.MaxSpawnSourceDistance <= config.MinSpawnSourceDistance)
                throw new InvalidOperationException("EncounterDirectorConfig.MaxSpawnSourceDistance must exceed MinSpawnSourceDistance.");
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
                throw new InvalidOperationException("Combat Director cell entity must exist before spawn source selection.");

            return directorEntity;
        }

        private static bool TrySelectSource(
            in CombatCell cell,
            EncounterDirectorConfig config,
            out SelectedSpawnSource selection)
        {
            var hasForward = TryReadAveragePlayerForward(out var playerForward);
            var found = false;
            var bestDirectionalScore = float.MaxValue;
            var bestDistanceScore = float.MaxValue;
            selection = default;

            foreach (var entity in SW.Query<All<SpawnSource>>().Entities())
            {
                ref readonly var source = ref entity.Read<SpawnSource>();
                if (!source.IsActive)
                    continue;

                if (!math.all(math.isfinite(source.Position)))
                    throw new InvalidOperationException("Spawn source position must be finite.");

                var distance = math.distance(cell.Center, source.Position);
                if (distance < config.MinSpawnSourceDistance || distance > config.MaxSpawnSourceDistance)
                    continue;

                var directionalScore = ScoreDirection(cell.Center, source.Position, playerForward, hasForward);
                if (directionalScore > bestDirectionalScore)
                    continue;

                var distanceScore = math.abs(distance - cell.Radius);
                if (math.abs(directionalScore - bestDirectionalScore) < 0.0001f
                    && distanceScore >= bestDistanceScore)
                    continue;

                selection = new SelectedSpawnSource
                {
                    CellId = cell.CellId,
                    SourceEntity = entity.GID,
                    SourceType = source.Type,
                    Position = source.Position
                };
                bestDirectionalScore = directionalScore;
                bestDistanceScore = distanceScore;
                found = true;
            }

            return found;
        }

        private static bool TryReadAveragePlayerForward(out float3 playerForward)
        {
            var forwardSum = float3.zero;
            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState>>().Entities())
            {
                var forward = player.Read<CharacterNetState>().Rotation * Vector3.forward;
                forwardSum += new float3(forward.x, 0f, forward.z);
            }

            if (math.lengthsq(forwardSum) < 0.0001f)
            {
                playerForward = new float3(0f, 0f, 1f);
                return false;
            }

            playerForward = math.normalize(forwardSum);
            return true;
        }

        private static float ScoreDirection(float3 cellCenter, float3 sourcePosition, float3 playerForward, bool hasForward)
        {
            if (!hasForward)
                return 0f;

            var offset = new float3(sourcePosition.x - cellCenter.x, 0f, sourcePosition.z - cellCenter.z);
            if (math.lengthsq(offset) < 0.0001f)
                return 1f;

            return math.dot(math.normalize(offset), playerForward);
        }

        private static bool IsSpawnPhase(DirectorPhase phase)
        {
            return phase == DirectorPhase.BuildUp || phase == DirectorPhase.Peak;
        }

        private static void ClearSelection(SW.Entity directorEntity)
        {
            if (directorEntity.Has<SelectedSpawnSource>())
                directorEntity.Delete<SelectedSpawnSource>();
        }
    }
}
