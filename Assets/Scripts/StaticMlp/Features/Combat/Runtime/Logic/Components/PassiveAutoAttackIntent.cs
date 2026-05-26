using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct PassiveAutoAttackIntent : IComponent
    {
        public CombatAbilityId AbilityId;
        public EntityGID Target;
        public uint ShotSequence;
        public float LocalFireTime;
    }
}
