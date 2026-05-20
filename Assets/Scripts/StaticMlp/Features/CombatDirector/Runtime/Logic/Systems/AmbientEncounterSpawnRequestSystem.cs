using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class AmbientEncounterSpawnRequestSystem : ISystem
    {
        public void Update()
        {
            var directorEntity = ReadDirectorEntity();
            ref readonly var state = ref directorEntity.Read<DirectorState>();
            if (!IsAmbientSpawnPhase(state.Phase)
                || directorEntity.Has<EncounterState>()
                || !directorEntity.Has<SelectedSpawnSource>()
                || HasPendingSpawnRequest())
            {
                return;
            }

            ref readonly var selection = ref directorEntity.Read<SelectedSpawnSource>();
            if (selection.AmbientKind == AmbientSpawnKind.None)
                return;

            var config = SW.GetResource<EncounterDirectorConfig>();
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            var aliveCount = CountAliveEnemies(in cell);
            var remainingSlots = config.AmbientMaxAliveEnemiesPerCell - aliveCount;
            if (remainingSlots <= 0)
                return;

            var count = ResolveAmbientCount(in selection, config, remainingSlots);
            if (count <= 0)
                return;

            SW.NewEntity<Default>().Set(new SpawnRequest
            {
                CellId = selection.CellId,
                SourceEntity = selection.SourceEntity,
                SourceType = selection.SourceType,
                SourceKind = selection.SourceKind,
                AmbientKind = selection.AmbientKind,
                Role = EnemyRole.Swarmer,
                Count = count,
                SpawnPosition = selection.Position
            });

            StartCooldown(in selection, config);
        }

        private static int ResolveAmbientCount(
            in SelectedSpawnSource selection,
            EncounterDirectorConfig config,
            int remainingSlots)
        {
            var maxForKind = selection.AmbientKind switch
            {
                AmbientSpawnKind.SoloAnimal => 1,
                AmbientSpawnKind.SoloBandit => 1,
                AmbientSpawnKind.SmallPack => 4,
                AmbientSpawnKind.Patrol => 4,
                AmbientSpawnKind.ResourceGuardian => 2,
                _ => throw new InvalidOperationException($"Unknown ambient spawn kind: {selection.AmbientKind}.")
            };

            var requestedMax = math.min(selection.AmbientMaxCount, maxForKind);
            requestedMax = math.min(requestedMax, config.AmbientMaxEnemiesPerRequest);
            requestedMax = math.min(requestedMax, remainingSlots);
            if (requestedMax < selection.AmbientMinCount)
                return 0;

            return requestedMax;
        }

        private static void StartCooldown(in SelectedSpawnSource selection, EncounterDirectorConfig config)
        {
            if (!selection.SourceEntity.TryUnpack<ServerWT>(out var sourceEntity))
                throw new InvalidOperationException("Selected ambient source must still exist when starting cooldown.");

            if (!sourceEntity.Has<AmbientSpawnMarker>())
                sourceEntity.Set(new AmbientSpawnMarker
                {
                    Kind = selection.AmbientKind,
                    MinCount = selection.AmbientMinCount,
                    MaxCount = selection.AmbientMaxCount,
                    CooldownSeconds = config.AmbientSpawnCooldownSeconds,
                    CooldownRemaining = config.AmbientSpawnCooldownSeconds
                });
            else
            {
                ref var marker = ref sourceEntity.Mut<AmbientSpawnMarker>();
                marker.CooldownRemaining = marker.CooldownSeconds;
            }
        }

        private static SW.Entity ReadDirectorEntity()
        {
            var found = false;
            SW.Entity directorEntity = default;

            foreach (var entity in SW.Query<All<CombatCell, DirectorState>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Combat Director currently supports only a single director cell.");

                directorEntity = entity;
                found = true;
            }

            if (!found)
                throw new InvalidOperationException("Combat Director cell entity must exist before ambient request building.");

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
