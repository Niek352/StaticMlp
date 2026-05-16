using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    [ReplicatedEvent(NetDelivery.UnreliableSequenced)]
    public struct DamageNumberEvent : IEvent
    {
        public const ushort NETWORK_EVENT_ID = 57023;

        public EntityGID Source;
        public EntityGID Target;
        public uint ClientCommandId;
        public float Value;
        public DamageType DamageType;
    }
}
