using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Build
{
    public readonly struct BuildModuleDefinition
    {
        public readonly BuildModuleId Id;
        public readonly EquipmentSlotKind SlotKind;
        public readonly BuildArchetypeId ArchetypeId;
        public readonly CombatAbilityId GrantedAbility;

        public BuildModuleSlotType SlotType
        {
            get
            {
                if (SlotKind == EquipmentSlotKind.Combat)
                    return BuildModuleSlotType.PrimaryAbility;

                throw new System.InvalidOperationException(
                    $"{nameof(BuildModuleSlotType)} compatibility mapping is only defined for {nameof(EquipmentSlotKind.Combat)} modules.");
            }
        }

        public BuildModuleDefinition(
            BuildModuleId id,
            EquipmentSlotKind slotKind,
            BuildArchetypeId archetypeId,
            CombatAbilityId grantedAbility)
        {
            Id = id;
            SlotKind = slotKind;
            ArchetypeId = archetypeId;
            GrantedAbility = grantedAbility;
        }
    }
}
