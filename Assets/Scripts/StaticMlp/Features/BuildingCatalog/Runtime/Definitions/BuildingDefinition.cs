using StaticMlp.Features.Settlement;
using Unity.Mathematics;

namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingDefinition
    {
        public readonly BuildingId Id;
        public readonly string Code;
        public readonly string DisplayName;
        public readonly BuildingCategory Category;
        public readonly string CategoryDisplayName;
        public readonly BuildingCapabilityFlags Capabilities;
        public readonly ResourceAmount[] ConstructionCost;
        public readonly int2 Footprint;
        public readonly float BuildWorkRequired;
        public readonly BuildingInteractionDefinition[] Interactions;
        public readonly BuildingNpcProfileDefinition NpcProfile;
        public readonly BuildingOperationDefinition Operation;

        public BuildingDefinition(
            BuildingId id,
            string code,
            string displayName,
            BuildingCategory category,
            string categoryDisplayName,
            BuildingCapabilityFlags capabilities,
            ResourceAmount[] constructionCost,
            int2 footprint,
            float buildWorkRequired,
            BuildingInteractionDefinition[] interactions,
            BuildingNpcProfileDefinition npcProfile,
            BuildingOperationDefinition operation)
        {
            Id = id;
            Code = code;
            DisplayName = displayName;
            Category = category;
            CategoryDisplayName = categoryDisplayName;
            Capabilities = capabilities;
            ConstructionCost = constructionCost;
            Footprint = footprint;
            BuildWorkRequired = buildWorkRequired;
            Interactions = interactions;
            NpcProfile = npcProfile;
            Operation = operation;
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
