using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EncounterRecoverySystem : ISystem
    {
        public void Update()
        {
            var directorEntity = ReadDirectorEntity();
            if (!directorEntity.Has<EncounterState>())
                return;

            ref readonly var encounter = ref directorEntity.Read<EncounterState>();
            if (encounter.AliveEnemyCount > 0)
                return;

            ref var state = ref directorEntity.Mut<DirectorState>();
            if (state.Phase == DirectorPhase.Contact || state.Phase == DirectorPhase.Suspicion)
            {
                state.Phase = DirectorPhase.Recovery;
                state.PhaseTimer = 0f;
            }

            directorEntity.Delete<EncounterState>();
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
                throw new InvalidOperationException("Combat Director cell entity must exist before encounter recovery.");

            return directorEntity;
        }
    }
}
