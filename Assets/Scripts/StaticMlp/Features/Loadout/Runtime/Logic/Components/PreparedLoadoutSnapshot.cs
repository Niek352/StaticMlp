using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Loadout
{
    public struct PreparedLoadoutSnapshot : IComponent
    {
        public LoadoutArchetypeId ArchetypeId;
        public LoadoutModuleId PrimaryModuleId;
        public CombatAbilityId PreparedAbilityId;
        public CombatAbilityId FallbackAbilityId;
    }
}
