using System;
using StaticMlp.Features.Effects;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    internal static class OpenWorldResourceHazardCatalog
    {
        private static readonly OpenWorldResourceHazardDefinition[] Definitions =
        {
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.TREE_RESOURCE_KIND), 2f, 6f, DamageType.Physical),
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.ORE_RESOURCE_KIND), 2.5f, 8f, DamageType.Physical),
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.SPORE_POD_RESOURCE_KIND), 2f, 5f, DamageType.Poison),
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.CHEST_RESOURCE_KIND), 2.75f, 9f, DamageType.Explosion)
        };

        public static ref readonly OpenWorldResourceHazardDefinition Get(ResourcePlacementKindId kindId)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].KindId != kindId)
                    continue;

                return ref Definitions[i];
            }

            throw new InvalidOperationException($"Missing hazard definition for resource placement kind {kindId.Value}.");
        }
    }
}
