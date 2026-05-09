using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.AiActions
{
    public sealed class AttackEnemyExecutor : AiTaskExecutorBase
    {
        private readonly AiTaskExecutionTransitions _transitions;

        public AttackEnemyExecutor(AiTaskExecutionTransitions transitions)
        {
            _transitions = transitions;
        }

        public override AiTaskType TaskType => AiTaskType.AttackEnemy;

        public override void Exit(SW.Entity entity, ref AiTaskState task)
        {
            if (entity.Has<AiAttackRequest>())
                entity.Delete<AiAttackRequest>();
        }

        public override void Execute(SW.Entity entity, ref AiTaskState task)
        {
            if (!AiBlackboardAccess.TryGetEntity(entity, AiCoreVariableIds.Enemy, out var enemyGid)
                || !enemyGid.TryUnpack<ServerWT>(out var enemy)
                || !enemy.Has<CharacterNetState>())
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var enemyState = ref enemy.Read<CharacterNetState>();
            AiBlackboardAccess.SetVector(entity, AiCoreVariableIds.LastKnownEnemyPosition, enemyState.Position);
            entity.Set(new AiMoveRequest
            {
                Destination = enemyState.Position,
                StopDistance = 1.8f
            });
            entity.Set(new AiAttackRequest
            {
                Target = enemyGid
            });
            task.Timer += Time.deltaTime;
        }
    }
}
