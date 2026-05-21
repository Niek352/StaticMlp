using System;
using System.Collections.Generic;
using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Loadout
{
    public static class LoadoutModuleCatalog
    {
        public static readonly LoadoutModuleId PoisonArrowModuleId = new(1);
        public static readonly LoadoutModuleId FireFlaskModuleId = new(2);
        public static readonly LoadoutModuleId ConstructionUtilityModuleId = new(101);
        public static readonly LoadoutModuleId LogisticsUtilityModuleId = new(102);
        public static readonly LoadoutModuleId BuilderPrioritySignalModuleId = new(201);
        public static readonly LoadoutModuleId HaulerPrioritySignalModuleId = new(202);
        public static readonly LoadoutModuleId StockpileCapacityModuleId = new(301);
        public static readonly LoadoutModuleId BedEfficiencyModuleId = new(302);
        public static readonly LoadoutModuleId RepairEfficiencyModuleId = new(303);
        public static readonly LoadoutModuleId ExtractionYieldModuleId = new(304);

        private static readonly LoadoutModuleDefinition[] Definitions =
        {
            new(
                PoisonArrowModuleId,
                EquipmentSlotKind.Combat,
                LoadoutArchetypeCatalog.PoisonArcherId,
                CombatAbilityId.PoisonArrow),
            new(
                FireFlaskModuleId,
                EquipmentSlotKind.Combat,
                LoadoutArchetypeCatalog.FireBomberId,
                CombatAbilityId.FireFlask),
            new(
                ConstructionUtilityModuleId,
                EquipmentSlotKind.Utility,
                LoadoutModuleEffectKind.ConstructionLogisticsUtility),
            new(
                LogisticsUtilityModuleId,
                EquipmentSlotKind.Utility,
                LoadoutModuleEffectKind.SettlementLogisticsUtility),
            new(
                BuilderPrioritySignalModuleId,
                EquipmentSlotKind.BuildSignal,
                LoadoutModuleEffectKind.BuilderPrioritySignal),
            new(
                HaulerPrioritySignalModuleId,
                EquipmentSlotKind.BuildSignal,
                LoadoutModuleEffectKind.HaulerPrioritySignal),
            new(
                StockpileCapacityModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.StorageCapacity),
            new(
                BedEfficiencyModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.BedEfficiency),
            new(
                RepairEfficiencyModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.RepairEfficiency),
            new(
                ExtractionYieldModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.ExtractionYield)
        };

        static LoadoutModuleCatalog()
        {
            SlotRules.ValidateModuleCatalog(Definitions);
        }

        public static IReadOnlyList<LoadoutModuleDefinition> All => Definitions;

        public static LoadoutModuleDefinition Get(LoadoutModuleId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(LoadoutModuleDefinition)} for build module id {id.Value} in {nameof(LoadoutModuleCatalog)}.");
        }

        public static bool TryGet(LoadoutModuleId id, out LoadoutModuleDefinition definition)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id != id)
                    continue;

                definition = Definitions[i];
                return true;
            }

            definition = default;
            return false;
        }
    }
}
