using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct CommandBotEvent : IEvent
    {
        public const ushort NETWORK_EVENT_ID = 56001;

        public EntityGID Bot;
        public ushort CommandType;
        public EntityGID Target;

        public CommandBotEvent(EntityGID bot, ushort commandType, EntityGID target)
        {
            Bot = bot;
            CommandType = commandType;
            Target = target;
        }
    }
}
