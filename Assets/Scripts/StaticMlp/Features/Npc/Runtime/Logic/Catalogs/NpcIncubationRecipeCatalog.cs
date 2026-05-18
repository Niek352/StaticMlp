using System;
using System.Collections.Generic;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Npc
{
    public static class NpcIncubationRecipeCatalog
    {
        public static readonly NpcIncubationRecipeId ResearcherRecipeId = new(1);

        private static readonly NpcIncubationRecipeDefinition[] Definitions =
        {
            new(
                ResearcherRecipeId,
                NpcDefinitionCatalog.IncubatedResearcherId,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 10),
                    new ResourceAmount(ResourceCatalog.StoneId, 5)
                },
                requiredStationTier: 1,
                durationSeconds: 60f)
        };

        static NpcIncubationRecipeCatalog()
        {
            NpcIncubationRecipeCatalogValidator.Validate(Definitions);
        }

        public static IReadOnlyList<NpcIncubationRecipeDefinition> All => Definitions;

        public static NpcIncubationRecipeDefinition Get(NpcIncubationRecipeId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(NpcIncubationRecipeDefinition)} for recipe id {id.Value} in {nameof(NpcIncubationRecipeCatalog)}.");
        }

        public static bool TryGet(NpcIncubationRecipeId id, out NpcIncubationRecipeDefinition definition)
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
