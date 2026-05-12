using StaticMlp.Features.Settlement;
using Unity.Mathematics;

namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingDefinition
    {
        public readonly BuildingId Id;
        public readonly ResourceAmount[] ConstructionCost;
        public readonly int2 Footprint;
        public readonly float BuildWorkRequired;

        public BuildingDefinition(
            BuildingId id,
            ResourceAmount[] constructionCost,
            int2 footprint,
            float buildWorkRequired)
        {
            Id = id;
            ConstructionCost = constructionCost;
            Footprint = footprint;
            BuildWorkRequired = buildWorkRequired;
        }

        public int FootprintWidth => Footprint.x;
        public int FootprintLength => Footprint.y;

        public int GetConstructionCost(ResourceId resourceId)
        {
            for (var i = 0; i < ConstructionCost.Length; i++)
            {
                if (ConstructionCost[i].Id == resourceId)
                    return ConstructionCost[i].Amount;
            }

            return 0;
        }
    }
}
