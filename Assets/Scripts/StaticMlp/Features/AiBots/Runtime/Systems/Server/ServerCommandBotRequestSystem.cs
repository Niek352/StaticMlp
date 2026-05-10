using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
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
                || !bot.Has<AiTaskState>()
                || !bot.Has<SW.Multi<AiBlackboardEntry>>()
                || !bot.Has<CharacterNetState>())
            {
                return;
            }

            var catalog = SW.GetResource<AiActionCatalog>();

            var commandedTask = (AiTaskType)request.CommandType;
            if (!catalog.SupportsManualCommand(commandedTask) || !CanCommandBot(sourcePeer, bot))
                return;

            if (!catalog.TryBindManualCommand(commandedTask, bot, request.Target))
                return;

            ref var brain = ref bot.Mut<AiBrain>();
            brain.CurrentTask = commandedTask;
            brain.NextDecisionTick = SW.GetResource<SimulationTime>().DeadlineAfter(0.5f);

            ref var task = ref bot.Mut<AiTaskState>();
            task.Task = commandedTask;
            task.Step = 0;
            task.ElapsedTicks = 0;
        }

        private static bool CanCommandBot(NetworkPeerId sourcePeer, SW.Entity bot)
        {
            ref readonly var botState = ref bot.Read<CharacterNetState>();
            return ServerPeerPlayers.IsPlayerNear(sourcePeer, botState.Position, 20f);
        }
    }
}
