using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public sealed class AttackEnemyActionPackage : IAiActionPackage
    {
        private static readonly AttackEnemyCollectVariables Collector = new();
        private static readonly AttackEnemyCommandTargetBinder CommandBinder = new();

        public AiTaskType TaskType => AiTaskType.AttackEnemy;
        public IAiActionVariableCollector VariableCollector => Collector;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => CommandBinder;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => AttackEnemyVariableBindings.Bindings;
        public IReadOnlyList<AiBehaviorTaskContribution> UtilityTaskContributions => AttackEnemyVariableBindings.UtilityTaskContributions;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new AttackEnemyExecutor(transitions);
        }

        private sealed class AttackEnemyCommandTargetBinder : IAiActionCommandTargetBinder
        {
            public bool TryBind(SW.Entity bot, EntityGID target)
            {
                if (!target.TryUnpack<ServerWT>(out var targetEntity) || !targetEntity.Has<CharacterNetState>())
                    return false;

                ref readonly var targetState = ref targetEntity.Read<CharacterNetState>();
                ref readonly var botState = ref bot.Read<CharacterNetState>();
                AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, target);
                AiBlackboardAccess.SetVector(bot, AiCoreVariableIds.LastKnownEnemyPosition, targetState.Position);
                AiBlackboardAccess.SetFloat(
                    bot,
                    AiCoreVariableIds.EnemyDistance,
                    (targetState.Position - botState.Position).magnitude);
                return true;
            }
        }
    }
}
