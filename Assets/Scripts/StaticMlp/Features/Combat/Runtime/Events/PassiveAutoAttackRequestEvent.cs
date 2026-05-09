using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct PassiveAutoAttackRequestEvent : IEvent
    {
        public EntityGID Target;
        public uint ShotSequence;

        public PassiveAutoAttackRequestEvent(EntityGID target, uint shotSequence)
        {
            Target = target;
            ShotSequence = shotSequence;
        }
    }
}
