using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class UnlockEvaluation
    {
        public static bool IsMet(in UnlockRequirement requirement, ISettlementUnlockReadModel state)
        {
            if (state == null)
                throw new System.InvalidOperationException($"{nameof(ISettlementUnlockReadModel)} is required for unlock evaluation.");

            switch (requirement.Kind)
            {
                case UnlockRequirementKind.None:
                    return true;

                case UnlockRequirementKind.SettlementLevel:
                    return state.SettlementLevel >= requirement.IntParameter;

                case UnlockRequirementKind.BuildingConstructed:
                    return state.HasConstructedBuilding(requirement.IntParameter);

                default:
                    throw new System.InvalidOperationException($"Unknown unlock requirement kind {requirement.Kind}.");
            }
        }
    }
}
