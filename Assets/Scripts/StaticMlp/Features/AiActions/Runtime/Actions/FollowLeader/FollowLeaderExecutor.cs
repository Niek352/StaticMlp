using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public sealed class FollowLeaderExecutor : AiTaskExecutorBase
    {
        private readonly AiTaskExecutionTransitions _transitions;

        public FollowLeaderExecutor(AiTaskExecutionTransitions transitions)
        {
            _transitions = transitions;
        }

        public override AiTaskType TaskType => AiTaskType.FollowLeader;

        public override void Execute(SW.Entity entity, ref AiTaskState task)
        {
            if (!AiBlackboardAccess.TryGetEntity(entity, AiCoreVariableIds.Leader, out var leaderGid)
                || !leaderGid.TryUnpack<ServerWT>(out var leader)
                || !leader.Has<CharacterNetState>())
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var leaderState = ref leader.Read<CharacterNetState>();
            entity.Set(new AiMoveRequest
            {
                Destination = leaderState.Position,
                StopDistance = 3f
            });
            task.ElapsedTicks++;
        }
    }
}
