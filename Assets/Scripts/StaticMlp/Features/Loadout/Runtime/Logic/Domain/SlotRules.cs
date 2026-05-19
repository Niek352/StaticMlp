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
                ValidateSlotKind(definitions[i].SlotKind);
        }
    }
}
