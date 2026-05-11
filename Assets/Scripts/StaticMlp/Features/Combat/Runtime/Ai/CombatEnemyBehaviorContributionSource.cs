using StaticMlp.Features.AiActions;
using StaticMlp.Features.AiBots;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatEnemyBehaviorContributionSource : IAiBehaviorContributionSource
    {
        public void Register(AiBehaviorTaskRegistry registry)
        {
            registry.Add(
                CombatEnemyBehaviorIds.Monster,
                AiTaskType.AttackEnemy,
                AttackEnemyVariableBindings.Considerations);
            registry.Add(
                CombatEnemyBehaviorIds.Monster,
                AiTaskType.Flee,
                FleeVariableBindings.Considerations);
            registry.Add(
                CombatEnemyBehaviorIds.Monster,
                AiTaskType.FollowLeader,
                FollowLeaderVariableBindings.Considerations);
        }
    }
}
