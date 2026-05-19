using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EnemyViewBindSystem : ISystem
    {
        public void Update()
        {
            foreach (var enemy in CW.Query<All<EnemyArchetype>>().Entities())
                ValidateViewRecipe(enemy);

            foreach (var enemy in CW.Query<All<EnemyArchetype>, None<EnemyRoleViewState>>().Entities())
                ApplyRoleState(enemy);

            foreach (var enemy in CW.Query<All<EnemyArchetype>, AllChanged<EnemyArchetype>, NoneAdded<EnemyArchetype>>().Entities())
                ApplyRoleState(enemy);
        }

        private static void ValidateViewRecipe(CW.Entity enemy)
        {
            if (!enemy.Has<ViewPath>())
                throw new InvalidOperationException("Replicated Combat Director enemies must get ViewPath from their client network archetype recipe.");

            if (!enemy.Has<ViewTransform>())
                throw new InvalidOperationException("Replicated Combat Director enemies must get ViewTransform from their client network archetype recipe.");
        }

        private static void ApplyRoleState(CW.Entity enemy)
        {
            ref readonly var archetype = ref enemy.Read<EnemyArchetype>();
            enemy.Set(new EnemyRoleViewState
            {
                Role = archetype.Role
            });
        }
    }
}
