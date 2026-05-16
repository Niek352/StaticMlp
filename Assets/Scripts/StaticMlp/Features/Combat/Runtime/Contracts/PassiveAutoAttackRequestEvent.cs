using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct PassiveAutoAttackRequestEvent : IEvent
    {
        public const ushort NETWORK_EVENT_ID = 57021;

        public EntityGID Target;
        public uint ShotSequence;

        public PassiveAutoAttackRequestEvent(EntityGID target, uint shotSequence)
        {
            Target = target;
            ShotSequence = shotSequence;
        }
    }
}
