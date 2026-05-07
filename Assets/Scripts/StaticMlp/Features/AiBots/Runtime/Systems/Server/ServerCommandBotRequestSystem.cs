using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerCommandBotRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<CommandBotEvent>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<CommandBotEvent>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var request in _requests)
                Handle(request.Value.SourcePeer, in request.Value.Value);
        }

        private static void Handle(NetworkPeerId sourcePeer, in CommandBotEvent request)
        {
            if (!request.Bot.TryUnpack<ServerWT>(out var bot)
                || !bot.Has<AiAgentTag>()
                || !bot.Has<AiBrain>()
                || !bot.Has<AiBlackboard>()
                || !bot.Has<CharacterNetState>())
            {
                return;
            }

            var commandedTask = (AiTaskType)request.CommandType;
            if (!IsSupportedCommand(commandedTask) || !CanCommandBot(sourcePeer, bot))
                return;

            ApplyCommandTarget(bot, commandedTask, request.Target);

            ref var brain = ref bot.Mut<AiBrain>();
            brain.CurrentTask = commandedTask;
            brain.DecisionCooldown = 0.5f;

            bot.Set(new AiTaskState
            {
                Task = commandedTask,
                Step = 0,
                Timer = 0f
            });
        }

        private static void ApplyCommandTarget(SW.Entity bot, AiTaskType task, EntityGID target)
        {
            ref var blackboard = ref bot.Mut<AiBlackboard>();

            if (task == AiTaskType.FollowLeader)
            {
                if (target.TryUnpack<ServerWT>(out _))
                    blackboard.Leader = target;

                return;
            }

            if (task != AiTaskType.AttackEnemy && task != AiTaskType.Flee)
                return;

            if (!target.TryUnpack<ServerWT>(out var targetEntity) || !targetEntity.Has<CharacterNetState>())
                return;

            blackboard.Enemy = target;
            ref readonly var targetState = ref targetEntity.Read<CharacterNetState>();
            ref readonly var botState = ref bot.Read<CharacterNetState>();
            blackboard.LastKnownEnemyPosition = targetState.Position;
            blackboard.EnemyDistance = (targetState.Position - botState.Position).magnitude;
        }

        private static bool CanCommandBot(NetworkPeerId sourcePeer, SW.Entity bot)
        {
            ref readonly var botState = ref bot.Read<CharacterNetState>();
            return ServerPeerPlayers.IsPlayerNear(sourcePeer, botState.Position, 20f);
        }

        private static bool IsSupportedCommand(AiTaskType task)
        {
            return task == AiTaskType.Idle
                   || task == AiTaskType.FollowLeader
                   || task == AiTaskType.AttackEnemy
                   || task == AiTaskType.Flee;
        }
    }
}
