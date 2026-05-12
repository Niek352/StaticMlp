using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Build
{
    public readonly struct BuildModuleDefinition
    {
        public readonly BuildModuleId Id;
        public readonly BuildModuleSlotType SlotType;
        public readonly BuildArchetypeId ArchetypeId;
        public readonly CombatAbilityId GrantedAbility;

        public BuildModuleDefinition(
            BuildModuleId id,
            BuildModuleSlotType slotType,
            BuildArchetypeId archetypeId,
            CombatAbilityId grantedAbility)
        {
            Id = id;
            SlotType = slotType;
            ArchetypeId = archetypeId;
            GrantedAbility = grantedAbility;
        }
    }
}
