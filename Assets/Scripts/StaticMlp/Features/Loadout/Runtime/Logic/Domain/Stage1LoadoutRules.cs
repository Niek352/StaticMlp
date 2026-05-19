using System;
using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Loadout
{
    public static class Stage1LoadoutRules
    {
        public static OwnerLoadoutSelection DefaultSelection()
        {
            return new OwnerLoadoutSelection
            {
                PrimaryModuleId = LoadoutModuleCatalog.PoisonArrowModuleId
            };
        }

        public static LoadoutModuleId NextPrimaryModule(LoadoutModuleId current)
        {
            if (current == LoadoutModuleCatalog.PoisonArrowModuleId)
                return LoadoutModuleCatalog.FireFlaskModuleId;

            return LoadoutModuleCatalog.PoisonArrowModuleId;
        }

        public static LoadoutModuleId PreviousPrimaryModule(LoadoutModuleId current)
        {
            if (current == LoadoutModuleCatalog.FireFlaskModuleId)
                return LoadoutModuleCatalog.PoisonArrowModuleId;

            return LoadoutModuleCatalog.FireFlaskModuleId;
        }

        public static PreparedLoadoutSnapshot CreatePreparedSnapshot(in OwnerLoadoutSelection selection)
        {
            if (!LoadoutModuleCatalog.TryGet(selection.PrimaryModuleId, out var module))
                throw new InvalidOperationException($"Missing {nameof(LoadoutModuleDefinition)} for build module id {selection.PrimaryModuleId.Value}.");

            return new PreparedLoadoutSnapshot
            {
                ArchetypeId = module.ArchetypeId,
                PrimaryModuleId = module.Id,
                PreparedAbilityId = module.GrantedAbility,
                FallbackAbilityId = CombatAbilityId.BasicMeleeAuto,
            };
        }

        public static bool IsAbilityPrepared(in PreparedLoadoutSnapshot snapshot, CombatAbilityId abilityId)
        {
            return abilityId == snapshot.PreparedAbilityId
                   || abilityId == snapshot.FallbackAbilityId;
        }
    }
}
