using System;
using StaticMlp.Features.Combat;

namespace StaticMlp.Features.Build
{
    public static class Stage1BuildRules
    {
        public static OwnerBuildSelection DefaultSelection()
        {
            return new OwnerBuildSelection
            {
                PrimaryModuleId = BuildModuleCatalog.PoisonArrowModuleId
            };
        }

        public static BuildModuleId NextPrimaryModule(BuildModuleId current)
        {
            if (current == BuildModuleCatalog.PoisonArrowModuleId)
                return BuildModuleCatalog.FireFlaskModuleId;

            return BuildModuleCatalog.PoisonArrowModuleId;
        }

        public static BuildModuleId PreviousPrimaryModule(BuildModuleId current)
        {
            if (current == BuildModuleCatalog.FireFlaskModuleId)
                return BuildModuleCatalog.PoisonArrowModuleId;

            return BuildModuleCatalog.FireFlaskModuleId;
        }

        public static PreparedBuildSnapshot CreatePreparedSnapshot(in OwnerBuildSelection selection)
        {
            if (!BuildModuleCatalog.TryGet(selection.PrimaryModuleId, out var module))
                throw new InvalidOperationException($"Missing {nameof(BuildModuleDefinition)} for build module id {selection.PrimaryModuleId.Value}.");

            return new PreparedBuildSnapshot
            {
                ArchetypeId = module.ArchetypeId,
                PrimaryModuleId = module.Id,
                PreparedAbilityId = module.GrantedAbility,
                FallbackAbilityId = CombatAbilityId.BasicMeleeAuto,
            };
        }

        public static bool IsAbilityPrepared(in PreparedBuildSnapshot snapshot, CombatAbilityId abilityId)
        {
            return abilityId == snapshot.PreparedAbilityId
                   || abilityId == snapshot.FallbackAbilityId;
        }
    }
}
