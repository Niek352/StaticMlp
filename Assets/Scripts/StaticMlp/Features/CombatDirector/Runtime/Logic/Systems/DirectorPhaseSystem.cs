using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class DirectorPhaseSystem : ISystem
    {
        private const int MIN_ALIVE_ENEMIES_TO_SUSTAIN_PEAK = 1;

        public void Update()
        {
            var config = SW.GetResource<EncounterDirectorConfig>();
            if (config.MinReliefSeconds <= 0f)
                throw new InvalidOperationException("EncounterDirectorConfig.MinReliefSeconds must be positive.");

            if (config.MinCooldownSeconds <= 0f)
                throw new InvalidOperationException("EncounterDirectorConfig.MinCooldownSeconds must be positive.");

            var directorEntity = ReadDirectorEntity();
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            ref readonly var budget = ref directorEntity.Read<ThreatBudget>();
            ref var state = ref directorEntity.Mut<DirectorState>();

            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            var nextPhaseTimer = state.PhaseTimer + deltaTime;
            var nextTimeSinceLastPeak = state.TimeSinceLastPeak + deltaTime;
            var nextPhase = state.Phase;

            switch (state.Phase)
            {
                case DirectorPhase.Calm:
                    if (budget.Current > config.BuildUpThreshold)
                        nextPhase = DirectorPhase.BuildUp;
                    break;
                case DirectorPhase.BuildUp:
                    if (budget.Current > config.PeakThreshold && HasActiveSpawnSource())
                        nextPhase = DirectorPhase.Peak;
                    break;
                case DirectorPhase.Peak:
                    if (CountAliveEnemies(in cell) < MIN_ALIVE_ENEMIES_TO_SUSTAIN_PEAK)
                        nextPhase = DirectorPhase.Relief;
                    break;
                case DirectorPhase.Relief:
                    if (nextPhaseTimer >= config.MinReliefSeconds)
                        nextPhase = DirectorPhase.Cooldown;
                    break;
                case DirectorPhase.Cooldown:
                    if (nextPhaseTimer >= config.MinCooldownSeconds)
                        nextPhase = DirectorPhase.Calm;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown director phase: {state.Phase}.");
            }

            if (nextPhase != state.Phase)
            {
                state.Phase = nextPhase;
                state.PhaseTimer = 0f;
                state.TimeSinceLastPeak = nextPhase == DirectorPhase.Peak
                    ? 0f
                    : nextTimeSinceLastPeak;
                return;
            }

            state.PhaseTimer = nextPhaseTimer;
            state.TimeSinceLastPeak = nextTimeSinceLastPeak;
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
                throw new InvalidOperationException("Combat Director cell entity must exist before phase updates.");

            return directorEntity;
        }

        private static bool HasActiveSpawnSource()
        {
            foreach (var entity in SW.Query<All<SpawnSource>>().Entities())
            {
                if (entity.Read<SpawnSource>().IsActive)
                    return true;
            }

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
