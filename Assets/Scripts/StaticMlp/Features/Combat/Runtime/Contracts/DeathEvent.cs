using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct DeathEvent : IEvent
    {
        public const ushort NETWORK_EVENT_ID = 57024;

        public EntityGID Source;
        public EntityGID Target;
        public uint ClientCommandId;
    }
}
