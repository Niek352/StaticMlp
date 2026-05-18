using System;
using System.Collections.Generic;
using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Build
{
    public static class BuildModuleCatalog
    {
        public static readonly BuildModuleId PoisonArrowModuleId = new(1);
        public static readonly BuildModuleId FireFlaskModuleId = new(2);

        private static readonly BuildModuleDefinition[] Definitions =
        {
            new(
                PoisonArrowModuleId,
                EquipmentSlotKind.Combat,
                BuildArchetypeCatalog.PoisonArcherId,
                CombatAbilityId.PoisonArrow),
            new(
                FireFlaskModuleId,
                EquipmentSlotKind.Combat,
                BuildArchetypeCatalog.FireBomberId,
                CombatAbilityId.FireFlask)
        };

        static BuildModuleCatalog()
        {
            SlotRules.ValidateModuleCatalog(Definitions);
        }

        public static IReadOnlyList<BuildModuleDefinition> All => Definitions;

        public static BuildModuleDefinition Get(BuildModuleId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(BuildModuleDefinition)} for build module id {id.Value} in {nameof(BuildModuleCatalog)}.");
        }

        public static bool TryGet(BuildModuleId id, out BuildModuleDefinition definition)
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
