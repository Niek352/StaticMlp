using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct CombatAbilityRequest : IComponent
    {
        public CombatAbilityId AbilityId;
        public EntityGID Source;
        public EntityGID Target;
        public uint ClientCommandId;
    }
}
