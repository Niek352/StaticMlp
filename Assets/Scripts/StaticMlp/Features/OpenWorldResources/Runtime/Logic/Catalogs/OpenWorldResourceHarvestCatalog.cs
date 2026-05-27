using System;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.OpenWorldResources
{
    internal static class OpenWorldResourceHarvestCatalog
    {
        private static readonly OpenWorldResourceHarvestDefinition[] Definitions =
        {
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.TREE_RESOURCE_KIND),
                5,
                1,
                new ResourceAmount(ResourceCatalog.WoodId, 1),
                1,
                10,
                OpenWorldResourceHarvestTag.Axe),
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.ORE_RESOURCE_KIND),
                8,
                1,
                new ResourceAmount(ResourceCatalog.OreId, 1),
                4,
                35,
                OpenWorldResourceHarvestTag.Blunt | OpenWorldResourceHarvestTag.Lightning),
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.SPORE_POD_RESOURCE_KIND),
                4,
                1,
                new ResourceAmount(ResourceCatalog.SporesId, 1),
                0,
                5,
                OpenWorldResourceHarvestTag.Fire),
            new(new ResourcePlacementKindId(OpenWorldGenerationConfig.CHEST_RESOURCE_KIND),
                6,
                1,
                new ResourceAmount(ResourceCatalog.ResinId, 1),
                2,
                20,
                OpenWorldResourceHarvestTag.Rune | OpenWorldResourceHarvestTag.Projectile)
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
