using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Build
{
    public struct PreparedBuildSnapshot : IComponent
    {
        public BuildArchetypeId ArchetypeId;
        public BuildModuleId PrimaryModuleId;
        public CombatAbilityId PreparedAbilityId;
        public CombatAbilityId FallbackAbilityId;
    }
}
