using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiTaskExecutionTransitions
    {
        public void SwitchToIdle(SW.Entity entity, ref AiTaskState task)
        {
            SwitchTo(entity, ref task, AiTaskType.Idle);
        }

        public void SwitchTo(SW.Entity entity, ref AiTaskState task, AiTaskType nextTask)
        {
            ref var brain = ref entity.Mut<AiBrain>();
            brain.CurrentTask = nextTask;
            task.Task = nextTask;
            task.Step = 0;
            task.Timer = 0f;
        }

        public void StopMovement(SW.Entity entity)
        {
            if (entity.Has<AiMoveRequest>())
                entity.Delete<AiMoveRequest>();

            ref readonly var state = ref entity.Read<CharacterNetState>();
            if (state.Velocity == Vector3.zero)
                return;

            ref var replicatedState = ref ReplicationMut.Mut<CharacterNetState>(entity);
            replicatedState.Velocity = Vector3.zero;
        }
    }
}
