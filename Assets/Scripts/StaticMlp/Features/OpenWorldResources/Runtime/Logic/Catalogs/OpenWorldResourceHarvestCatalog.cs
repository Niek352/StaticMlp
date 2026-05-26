using System;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.OpenWorldResources
{
    internal static class OpenWorldResourceHarvestCatalog
    {
        private static readonly OpenWorldResourceHarvestDefinition[] Definitions =
        {
            new(new ResourcePlacementKindId(1), 5, 1, new ResourceAmount(ResourceCatalog.WoodId, 1)),
            new(new ResourcePlacementKindId(2), 8, 1, new ResourceAmount(ResourceCatalog.StoneId, 1))
        };

        public static ref readonly OpenWorldResourceHarvestDefinition Get(ResourcePlacementKindId kindId)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].KindId != kindId)
                    continue;

                return ref Definitions[i];
            }

            throw new InvalidOperationException($"Missing harvest definition for resource placement kind {kindId.Value}.");
        }
    }
}
