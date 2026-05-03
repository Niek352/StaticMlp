namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingDefinition
    {
        public readonly BuildingId Id;
        public readonly string DisplayName;
        public readonly int CostWood;
        public readonly int CostStone;
        public readonly int FootprintWidth;
        public readonly int FootprintLength;
        public readonly float BuildWorkRequired;

        public BuildingDefinition(
            BuildingId id,
            string displayName,
            int costWood,
            int costStone,
            int footprintWidth,
            int footprintLength,
            float buildWorkRequired)
        {
            Id = id;
            DisplayName = displayName;
            CostWood = costWood;
            CostStone = costStone;
            FootprintWidth = footprintWidth;
            FootprintLength = footprintLength;
            BuildWorkRequired = buildWorkRequired;
        }
    }
}
