using System.Collections.Generic;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public static class AvailableRecipesQuery
    {
        public static IEnumerable<ProductionRecipeDefinition> Filter(IEnumerable<ProductionRecipeDefinition> definitions)
        {
            var state = ResolveState();
            foreach (var definition in definitions)
            {
                if (UnlockEvaluation.IsMet(in definition.UnlockRequirement, state))
                    yield return definition;
            }
        }

        public static bool IsAvailable(in ProductionRecipeDefinition definition)
        {
            var state = ResolveState();
            return UnlockEvaluation.IsMet(in definition.UnlockRequirement, state);
        }

        private static ISettlementUnlockReadModel ResolveState()
        {
            return CW.GetResource<ClientSettlementUnlockState>();
        }
    }
}
