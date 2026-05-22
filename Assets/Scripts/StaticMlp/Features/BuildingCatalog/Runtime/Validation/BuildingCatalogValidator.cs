using System;
using System.Collections.Generic;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingCatalogValidator
    {
        private const BuildingCapabilityFlags OPERATION_PROFILE_CAPABILITIES =
            BuildingCapabilityFlags.ProvidesStorage
            | BuildingCapabilityFlags.ProvidesWorkplace
            | BuildingCapabilityFlags.ProducesResources
            | BuildingCapabilityFlags.ConsumesResources
            | BuildingCapabilityFlags.ExtractsFromNode
            | BuildingCapabilityFlags.RequiresFuel
            | BuildingCapabilityFlags.ProvidesRest
            | BuildingCapabilityFlags.OpensQueue;

        public static void Validate(IReadOnlyList<BuildingDefinition> definitions)
        {
            var ids = new HashSet<BuildingId>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];

                if (!ids.Add(definition.Id))
                    throw new InvalidOperationException($"Duplicate building id {definition.Id.Value}.");

                if (string.IsNullOrWhiteSpace(definition.Code))
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has no code.");

                if (string.IsNullOrWhiteSpace(definition.DisplayName))
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has no display name.");

                if (definition.Category == BuildingCategory.None)
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has no category.");

                if (definition.FootprintWidth <= 0 || definition.FootprintLength <= 0)
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has invalid footprint {definition.FootprintWidth}x{definition.FootprintLength}.");

                if (definition.BuildWorkRequired <= 0f)
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has non-positive build work {definition.BuildWorkRequired}.");

                ValidateConstructionCost(in definition);
                ValidateInteractions(in definition);
                ValidateProfiles(in definition);
                ValidateOperationOutput(in definition);
            }
        }

        private static void ValidateConstructionCost(in BuildingDefinition definition)
        {
            if (definition.ConstructionCost == null || definition.ConstructionCost.Length == 0)
                throw new InvalidOperationException($"Building id {definition.Id.Value} has no construction cost.");

            var costIds = new HashSet<ResourceId>();
            for (var i = 0; i < definition.ConstructionCost.Length; i++)
            {
                var cost = definition.ConstructionCost[i];
                if (!costIds.Add(cost.Id))
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has duplicate construction cost resource {cost.Id.Value}.");

                if (cost.Amount <= 0)
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has non-positive construction cost for resource {cost.Id.Value}.");

                ref readonly var resource = ref ResourceCatalog.Get(cost.Id);
                if (!resource.Usage.HasFlag(ResourceUsageFlags.Construction))
                    throw new InvalidOperationException($"Building id {definition.Id.Value} uses non-construction resource {cost.Id.Value} in construction cost.");
            }
        }

        private static void ValidateInteractions(in BuildingDefinition definition)
        {
            if (definition.Capabilities.HasFlag(BuildingCapabilityFlags.SupportsPlayerInteraction)
                && (definition.Interactions == null || definition.Interactions.Length == 0))
                throw new InvalidOperationException($"Building id {definition.Id.Value} supports player interaction but has no interactions.");

            if (definition.Interactions == null)
                return;

            var kinds = new HashSet<BuildingInteractionKind>();
            for (var i = 0; i < definition.Interactions.Length; i++)
            {
                var interaction = definition.Interactions[i];
                if (interaction.Kind == BuildingInteractionKind.None)
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has an interaction with no kind.");

                if (!kinds.Add(interaction.Kind))
                    throw new InvalidOperationException($"Building id {definition.Id.Value} has duplicate interaction {interaction.Kind}.");
            }
        }

        private static void ValidateProfiles(in BuildingDefinition definition)
        {
            if ((definition.Capabilities & OPERATION_PROFILE_CAPABILITIES) != BuildingCapabilityFlags.None
                && !definition.Operation.IsDefined)
                throw new InvalidOperationException($"Building id {definition.Id.Value} has operation capabilities without an operation profile.");

            if (definition.Capabilities.HasFlag(BuildingCapabilityFlags.SupportsNpcInteraction)
                && !definition.NpcProfile.IsDefined)
                throw new InvalidOperationException($"Building id {definition.Id.Value} supports NPC interaction without an NPC profile.");
        }

        private static void ValidateOperationOutput(in BuildingDefinition definition)
        {
            var extractsFromNode = definition.Capabilities.HasFlag(BuildingCapabilityFlags.ExtractsFromNode)
                                   || definition.Operation.OperationCapabilities.HasFlag(BuildingCapabilityFlags.ExtractsFromNode);

            if (!extractsFromNode)
            {
                if (definition.Operation.OutputResourceId.Value != 0)
                    throw new InvalidOperationException($"Building id {definition.Id.Value} defines output resource without extraction capability.");

                return;
            }

            if (definition.Operation.OutputResourceId.Value == 0)
                throw new InvalidOperationException($"Extraction building id {definition.Id.Value} has no output resource.");

            ref readonly var output = ref ResourceCatalog.Get(definition.Operation.OutputResourceId);
            if (!output.Usage.HasFlag(ResourceUsageFlags.ProductionOutput))
                throw new InvalidOperationException(
                    $"Extraction building id {definition.Id.Value} outputs non-production resource {output.Id.Value}.");
        }
    }
}
