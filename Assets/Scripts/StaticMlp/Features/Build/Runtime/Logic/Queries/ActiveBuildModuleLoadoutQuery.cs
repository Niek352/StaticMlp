using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Build
{
    public static class ActiveBuildModuleLoadoutQuery
    {
        public static BuildModuleId Get(in ActiveBuildModuleLoadout loadout, EquipmentSlotKind slotKind, int slotIndex)
        {
            switch (slotKind)
            {
                case EquipmentSlotKind.Combat:
                    if (slotIndex == 0) return loadout.Combat0;
                    if (slotIndex == 1) return loadout.Combat1;
                    if (slotIndex == 2) return loadout.Combat2;
                    throw SlotOutOfRange(slotKind, slotIndex);
                case EquipmentSlotKind.Utility:
                    if (slotIndex == 0) return loadout.Utility0;
                    if (slotIndex == 1) return loadout.Utility1;
                    throw SlotOutOfRange(slotKind, slotIndex);
                case EquipmentSlotKind.BuildSignal:
                    if (slotIndex == 0) return loadout.BuildSignal0;
                    if (slotIndex == 1) return loadout.BuildSignal1;
                    throw SlotOutOfRange(slotKind, slotIndex);
                case EquipmentSlotKind.BaseInfrastructure:
                    if (slotIndex == 0) return loadout.BaseInfrastructure0;
                    if (slotIndex == 1) return loadout.BaseInfrastructure1;
                    if (slotIndex == 2) return loadout.BaseInfrastructure2;
                    if (slotIndex == 3) return loadout.BaseInfrastructure3;
                    throw SlotOutOfRange(slotKind, slotIndex);
                default:
                    throw new InvalidOperationException($"Unsupported equipment slot kind {slotKind}.");
            }
        }

        public static bool Contains(in ActiveBuildModuleLoadout loadout, BuildModuleId moduleId)
        {
            return moduleId.Value != 0
                   && (loadout.Combat0 == moduleId
                       || loadout.Combat1 == moduleId
                       || loadout.Combat2 == moduleId
                       || loadout.Utility0 == moduleId
                       || loadout.Utility1 == moduleId
                       || loadout.BuildSignal0 == moduleId
                       || loadout.BuildSignal1 == moduleId
                       || loadout.BaseInfrastructure0 == moduleId
                       || loadout.BaseInfrastructure1 == moduleId
                       || loadout.BaseInfrastructure2 == moduleId
                       || loadout.BaseInfrastructure3 == moduleId);
        }

        public static void CopyActiveModules(in ActiveBuildModuleLoadout loadout, List<BuildModuleId> results)
        {
            results.Clear();
            AddIfSet(results, loadout.Combat0);
            AddIfSet(results, loadout.Combat1);
            AddIfSet(results, loadout.Combat2);
            AddIfSet(results, loadout.Utility0);
            AddIfSet(results, loadout.Utility1);
            AddIfSet(results, loadout.BuildSignal0);
            AddIfSet(results, loadout.BuildSignal1);
            AddIfSet(results, loadout.BaseInfrastructure0);
            AddIfSet(results, loadout.BaseInfrastructure1);
            AddIfSet(results, loadout.BaseInfrastructure2);
            AddIfSet(results, loadout.BaseInfrastructure3);
        }

        private static void AddIfSet(List<BuildModuleId> results, BuildModuleId moduleId)
        {
            if (moduleId.Value != 0)
                results.Add(moduleId);
        }

        private static InvalidOperationException SlotOutOfRange(EquipmentSlotKind slotKind, int slotIndex)
        {
            return new InvalidOperationException($"Slot index {slotIndex} is outside {slotKind} slot limit {SlotRuleCatalog.GetLimit(slotKind)}.");
        }
    }
}
