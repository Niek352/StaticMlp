using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public sealed class FollowLeaderActionPackage : IAiActionPackage
    {
        private static readonly FollowLeaderCommandTargetBinder CommandBinder = new();

        public AiTaskType TaskType => AiTaskType.FollowLeader;
        public IAiActionVariableCollector VariableCollector => null;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => CommandBinder;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => FollowLeaderVariableBindings.Bindings;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new FollowLeaderExecutor(transitions);
        }

        private sealed class FollowLeaderCommandTargetBinder : IAiActionCommandTargetBinder
        {
            public bool TryBind(SW.Entity bot, EntityGID target)
            {
                if (!target.TryUnpack<ServerWT>(out _))
                    return false;

                AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Leader, target);
                return true;
            }
        }
    }
}
