using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public struct DeathEvent : IEvent
    {
        public EntityGID Source;
        public EntityGID Target;
        public uint ClientCommandId;
    }
}
