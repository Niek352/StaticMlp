using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EncounterLifetimeSystem : ISystem
    {
        public void Update()
        {
            var directorEntity = ReadDirectorEntity();
            if (!directorEntity.Has<EncounterState>())
                return;

            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            ref var encounter = ref directorEntity.Mut<EncounterState>();
            encounter.TimeAlive += SW.GetResource<SimulationTime>().FixedStepSeconds;
            encounter.AliveEnemyCount = CountAliveEnemies(in cell);
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
                throw new InvalidOperationException("Combat Director cell entity must exist before encounter lifetime updates.");

            return directorEntity;
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
