using System;
using System.Collections.Generic;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Npc
{
    public static class NpcIncubationRecipeCatalogValidator
    {
        public static void Validate(IReadOnlyList<NpcIncubationRecipeDefinition> definitions)
        {
            var ids = new HashSet<NpcIncubationRecipeId>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];

                if (!ids.Add(definition.Id))
                    throw new InvalidOperationException($"Duplicate incubation recipe id {definition.Id.Value}.");

                ValidateResultNpc(definition);
                ValidateDuration(definition);
                ValidateStationTier(definition);
                ValidateCosts(definition);
            }
        }

        private static void ValidateResultNpc(in NpcIncubationRecipeDefinition definition)
        {
            var npcDefinition = NpcDefinitionCatalog.Get(definition.ResultNpc);

            if ((npcDefinition.AllowedAcquisitionPaths & NpcAcquisitionPathFlags.Incubation) == 0)
            {
                throw new InvalidOperationException(
                    $"Incubation recipe {definition.Id.Value} result NPC {definition.ResultNpc.Value} does not allow incubation.");
            }
        }

        private static void ValidateDuration(in NpcIncubationRecipeDefinition definition)
        {
            if (definition.DurationSeconds <= 0f)
                throw new InvalidOperationException($"Incubation recipe {definition.Id.Value} has zero or negative duration.");
        }

        private static void ValidateStationTier(in NpcIncubationRecipeDefinition definition)
        {
            if (definition.RequiredStationTier == 0)
                throw new InvalidOperationException($"Incubation recipe {definition.Id.Value} has zero station tier.");
        }

        private static void ValidateCosts(in NpcIncubationRecipeDefinition definition)
        {
            if (definition.Costs == null || definition.Costs.Length == 0)
                throw new InvalidOperationException($"Incubation recipe {definition.Id.Value} has empty cost list.");

            for (var i = 0; i < definition.Costs.Length; i++)
            {
                var cost = definition.Costs[i];
                _ = ResourceCatalog.Get(cost.Id);

                if (cost.Amount <= 0)
                {
                    throw new InvalidOperationException(
                        $"Incubation recipe {definition.Id.Value} cost at index {i} has non-positive amount.");
                }
            }
        }
    }
}
