using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct CombatTargetHitEvent : IEvent
    {
        public EntityGID Source;
        public EntityGID Target;
        public CombatAbilityId AbilityId;
        public uint ClientCommandId;
    }
}
