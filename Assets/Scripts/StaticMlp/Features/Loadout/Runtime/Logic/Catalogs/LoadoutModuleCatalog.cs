using System;
using System.Collections.Generic;
using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Loadout
{
    public static class LoadoutModuleCatalog
    {
        public static readonly LoadoutModuleId PoisonArrowModuleId = new(1);
        public static readonly LoadoutModuleId FireFlaskModuleId = new(2);

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
                CombatAbilityId.FireFlask)
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
