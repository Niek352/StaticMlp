using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Loadout
{
    public static class SlotRules
    {
        public static void ValidateSlotKind(EquipmentSlotKind kind)
        {
            SlotRuleCatalog.GetLimit(kind);
        }

        public static void ValidateSlotIndex(EquipmentSlotKind kind, int slotIndex)
        {
            var limit = SlotRuleCatalog.GetLimit(kind);
            if (slotIndex < 0 || slotIndex >= limit)
                throw new InvalidOperationException($"Slot index {slotIndex} is outside {kind} slot limit {limit}.");
        }

        public static void ValidateModuleSupportsSlotKind(in LoadoutModuleDefinition module, EquipmentSlotKind requestedKind)
        {
            ValidateSlotKind(requestedKind);

            if (module.SlotKind != requestedKind)
            {
                throw new InvalidOperationException(
                    $"Build module id {module.Id.Value} supports {module.SlotKind}, not requested slot kind {requestedKind}.");
            }
        }

        public static void ValidateModuleCatalog(IReadOnlyList<LoadoutModuleDefinition> definitions)
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                ValidateSlotKind(definition.SlotKind);
                ValidateModuleDefinition(in definition);

                for (var j = i + 1; j < definitions.Count; j++)
                {
                    if (definition.Id == definitions[j].Id)
                        throw new InvalidOperationException($"Duplicate build module id {definition.Id.Value}.");
                }
            }
        }

        private static void ValidateModuleDefinition(in LoadoutModuleDefinition definition)
        {
            if (definition.Id.Value == 0)
                throw new InvalidOperationException("Build module id 0 is reserved.");

            if (definition.EffectKind == LoadoutModuleEffectKind.None)
                throw new InvalidOperationException($"Build module id {definition.Id.Value} has no effect kind.");

            if (definition.SlotKind == EquipmentSlotKind.Combat)
            {
                if (!definition.HasArchetype)
                    throw new InvalidOperationException($"Combat build module id {definition.Id.Value} requires a build archetype.");

                if (!definition.HasGrantedAbility)
                    throw new InvalidOperationException($"Combat build module id {definition.Id.Value} requires a combat ability.");

                if (definition.EffectKind != LoadoutModuleEffectKind.CombatAbility)
                    throw new InvalidOperationException($"Combat build module id {definition.Id.Value} must use {nameof(LoadoutModuleEffectKind.CombatAbility)}.");

                return;
            }

            if (definition.HasArchetype)
                throw new InvalidOperationException($"Non-combat build module id {definition.Id.Value} must not bind a combat archetype.");

            if (definition.HasGrantedAbility)
                throw new InvalidOperationException($"Non-combat build module id {definition.Id.Value} must not bind a combat ability.");

            if (definition.EffectKind == LoadoutModuleEffectKind.CombatAbility)
                throw new InvalidOperationException($"Non-combat build module id {definition.Id.Value} must not use {nameof(LoadoutModuleEffectKind.CombatAbility)}.");
        }
    }
}
