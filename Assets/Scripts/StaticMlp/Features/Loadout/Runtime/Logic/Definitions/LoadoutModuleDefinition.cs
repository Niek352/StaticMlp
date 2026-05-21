using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Loadout
{
    public readonly struct LoadoutModuleDefinition
    {
        public readonly LoadoutModuleId Id;
        public readonly EquipmentSlotKind SlotKind;
        public readonly LoadoutArchetypeId ArchetypeId;
        public readonly CombatAbilityId GrantedAbility;
        public readonly LoadoutModuleEffectKind EffectKind;
        public readonly bool HasArchetype;
        public readonly bool HasGrantedAbility;

        public LoadoutModuleSlotType SlotType
        {
            get
            {
                if (SlotKind == EquipmentSlotKind.Combat)
                    return LoadoutModuleSlotType.PrimaryAbility;

                throw new System.InvalidOperationException(
                    $"{nameof(LoadoutModuleSlotType)} compatibility mapping is only defined for {nameof(EquipmentSlotKind.Combat)} modules.");
            }
        }

        public LoadoutModuleDefinition(
            LoadoutModuleId id,
            EquipmentSlotKind slotKind,
            LoadoutArchetypeId archetypeId,
            CombatAbilityId grantedAbility)
        {
            Id = id;
            SlotKind = slotKind;
            ArchetypeId = archetypeId;
            GrantedAbility = grantedAbility;
            EffectKind = LoadoutModuleEffectKind.CombatAbility;
            HasArchetype = true;
            HasGrantedAbility = true;
        }

        public LoadoutModuleDefinition(
            LoadoutModuleId id,
            EquipmentSlotKind slotKind,
            LoadoutModuleEffectKind effectKind)
        {
            Id = id;
            SlotKind = slotKind;
            ArchetypeId = default;
            GrantedAbility = default;
            EffectKind = effectKind;
            HasArchetype = false;
            HasGrantedAbility = false;
        }
    }
}
