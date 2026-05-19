using System;

namespace StaticMlp.Features.Build
{
    public static class ActiveBuildModuleLoadoutRules
    {
        public static void ValidateCanActivate(
            in ActiveBuildModuleLoadout loadout,
            in BuildModuleDefinition module,
            EquipmentSlotKind slotKind,
            int slotIndex)
        {
            SlotRules.ValidateSlotIndex(slotKind, slotIndex);
            SlotRules.ValidateModuleSupportsSlotKind(module, slotKind);

            var current = ActiveBuildModuleLoadoutQuery.Get(loadout, slotKind, slotIndex);
            if (current.Value != 0 && current != module.Id)
            {
                throw new InvalidOperationException(
                    $"{slotKind} slot {slotIndex} already has active build module id {current.Value}.");
            }

            if (ActiveBuildModuleLoadoutQuery.Contains(loadout, module.Id) && current != module.Id)
                throw new InvalidOperationException($"Build module id {module.Id.Value} is already active.");
        }

        public static bool CanActivate(
            in ActiveBuildModuleLoadout loadout,
            in BuildModuleDefinition module,
            EquipmentSlotKind slotKind,
            int slotIndex)
        {
            if (slotIndex < 0)
                return false;

            int limit;
            try
            {
                limit = SlotRuleCatalog.GetLimit(slotKind);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            if (slotIndex >= limit || module.SlotKind != slotKind)
                return false;

            var current = ActiveBuildModuleLoadoutQuery.Get(loadout, slotKind, slotIndex);
            if (current.Value != 0 && current != module.Id)
                return false;

            return !ActiveBuildModuleLoadoutQuery.Contains(loadout, module.Id) || current == module.Id;
        }

        public static void Activate(
            ref ActiveBuildModuleLoadout loadout,
            in BuildModuleDefinition module,
            EquipmentSlotKind slotKind,
            int slotIndex)
        {
            ValidateCanActivate(loadout, module, slotKind, slotIndex);
            Set(ref loadout, slotKind, slotIndex, module.Id);
        }

        public static void Deactivate(ref ActiveBuildModuleLoadout loadout, EquipmentSlotKind slotKind, int slotIndex)
        {
            SlotRules.ValidateSlotIndex(slotKind, slotIndex);
            Set(ref loadout, slotKind, slotIndex, default);
        }

        public static void Set(
            ref ActiveBuildModuleLoadout loadout,
            EquipmentSlotKind slotKind,
            int slotIndex,
            BuildModuleId moduleId)
        {
            switch (slotKind)
            {
                case EquipmentSlotKind.Combat:
                    if (slotIndex == 0) loadout.Combat0 = moduleId;
                    else if (slotIndex == 1) loadout.Combat1 = moduleId;
                    else if (slotIndex == 2) loadout.Combat2 = moduleId;
                    else SlotRules.ValidateSlotIndex(slotKind, slotIndex);
                    return;
                case EquipmentSlotKind.Utility:
                    if (slotIndex == 0) loadout.Utility0 = moduleId;
                    else if (slotIndex == 1) loadout.Utility1 = moduleId;
                    else SlotRules.ValidateSlotIndex(slotKind, slotIndex);
                    return;
                case EquipmentSlotKind.BuildSignal:
                    if (slotIndex == 0) loadout.BuildSignal0 = moduleId;
                    else if (slotIndex == 1) loadout.BuildSignal1 = moduleId;
                    else SlotRules.ValidateSlotIndex(slotKind, slotIndex);
                    return;
                case EquipmentSlotKind.BaseInfrastructure:
                    if (slotIndex == 0) loadout.BaseInfrastructure0 = moduleId;
                    else if (slotIndex == 1) loadout.BaseInfrastructure1 = moduleId;
                    else if (slotIndex == 2) loadout.BaseInfrastructure2 = moduleId;
                    else if (slotIndex == 3) loadout.BaseInfrastructure3 = moduleId;
                    else SlotRules.ValidateSlotIndex(slotKind, slotIndex);
                    return;
                default:
                    SlotRules.ValidateSlotKind(slotKind);
                    return;
            }
        }
    }
}
