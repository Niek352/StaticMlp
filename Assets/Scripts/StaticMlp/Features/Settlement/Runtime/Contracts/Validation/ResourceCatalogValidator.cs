using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class ResourceCatalogValidator
    {
        public static void Validate(IReadOnlyList<ResourceDefinition> definitions)
        {
            var ids = new HashSet<ResourceId>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];

                if (!ids.Add(definition.Id))
                    throw new InvalidOperationException($"Duplicate settlement resource id {definition.Id.Value}.");

                if (string.IsNullOrWhiteSpace(definition.DisplayName))
                    throw new InvalidOperationException($"Settlement resource id {definition.Id.Value} has no display name.");

                if (definition.Family == ResourceFamily.None)
                    throw new InvalidOperationException($"Settlement resource id {definition.Id.Value} has no resource family.");

                if (definition.Usage == ResourceUsageFlags.None)
                    throw new InvalidOperationException($"Settlement resource id {definition.Id.Value} has no resource usage flags.");

                if (definition.IsSettlementStored && definition.StartingSettlementAmount < 0)
                {
                    throw new InvalidOperationException(
                        $"Settlement resource id {definition.Id.Value} has negative starting settlement amount {definition.StartingSettlementAmount}.");
                }
            }
        }
    }
}
