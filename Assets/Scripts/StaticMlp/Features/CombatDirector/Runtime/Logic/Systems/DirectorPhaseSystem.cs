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
        public void Update()
        {
            var config = SW.GetResource<EncounterDirectorConfig>();
            if (config.MinReliefSeconds <= 0f)
                throw new InvalidOperationException("EncounterDirectorConfig.MinReliefSeconds must be positive.");

            if (config.MinCooldownSeconds <= 0f)
                throw new InvalidOperationException("EncounterDirectorConfig.MinCooldownSeconds must be positive.");

            var directorEntity = ReadDirectorEntity();
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            ref readonly var attention = ref directorEntity.Read<CellAttention>();
            ref var state = ref directorEntity.Mut<DirectorState>();

            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            var nextPhaseTimer = state.PhaseTimer + deltaTime;
            var nextTimeSinceLastPressureEvent = state.TimeSinceLastPressureEvent + deltaTime;
            var nextPhase = state.Phase;
            var hasEncounter = directorEntity.Has<EncounterState>();
            var hasSuspicion = HasSuspicionAttention(in attention);

            switch (state.Phase)
            {
                case DirectorPhase.Dormant:
                    if (HasActivePlayer())
                        nextPhase = DirectorPhase.Ambient;
                    break;
                case DirectorPhase.Ambient:
                    if (hasEncounter)
                        nextPhase = DirectorPhase.Contact;
                    else if (hasSuspicion)
                        nextPhase = DirectorPhase.Suspicion;
                    break;
                case DirectorPhase.Contact:
                    if (!hasEncounter)
                        nextPhase = DirectorPhase.Recovery;
                    else if (hasSuspicion)
                        nextPhase = DirectorPhase.Suspicion;
                    break;
                case DirectorPhase.Suspicion:
                    if (hasEncounter)
                        nextPhase = DirectorPhase.Contact;
                    else if (!hasSuspicion)
                        nextPhase = DirectorPhase.Recovery;
                    break;
                case DirectorPhase.Escalation:
                    break;
                case DirectorPhase.PressureEvent:
                    if (CountAliveEnemies(in cell) == 0)
                        nextPhase = DirectorPhase.Recovery;
                    break;
                case DirectorPhase.Recovery:
                    if (nextPhaseTimer >= config.MinReliefSeconds)
                        nextPhase = DirectorPhase.Cooldown;
                    break;
                case DirectorPhase.Cooldown:
                    if (nextPhaseTimer >= config.MinCooldownSeconds && !hasEncounter && !hasSuspicion)
                        nextPhase = DirectorPhase.Ambient;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown director phase: {state.Phase}.");
            }

            if (nextPhase != state.Phase)
            {
                state.Phase = nextPhase;
                state.PhaseTimer = 0f;
                state.TimeSinceLastPressureEvent = nextPhase == DirectorPhase.PressureEvent
                    ? 0f
                    : nextTimeSinceLastPressureEvent;
                return;
            }

            state.PhaseTimer = nextPhaseTimer;
            state.TimeSinceLastPressureEvent = nextTimeSinceLastPressureEvent;
        }

        private static SW.Entity ReadDirectorEntity()
        {
            var found = false;
            SW.Entity directorEntity = default;

            foreach (var entity in SW.Query<All<CombatCell, CellAttention, DirectorState>>().Entities())
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

        private static bool HasActivePlayer()
        {
            foreach (var _ in SW.Query<All<PlayerTag, CharacterNetState>>().Entities())
                return true;

            return false;
        }

        private static bool HasSuspicionAttention(in CellAttention attention)
        {
            return attention.Noise > 0f
                   || attention.Trespass > 0f
                   || attention.Combat > 0f
                   || attention.Loot > 0f
                   || attention.FactionAlarm > 0f;
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
