using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiBots
{
    public struct CommandBotEvent : IEvent
    {
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
