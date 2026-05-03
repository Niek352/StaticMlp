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
        public readonly ushort BlueprintArchetypeId;
        public readonly ushort FinishedArchetypeId;
        public readonly string GhostPreviewViewPath;
        public readonly string BlueprintViewPath;
        public readonly string FinishedViewPath;

        public BuildingDefinition(
            BuildingId id,
            string displayName,
            int costWood,
            int costStone,
            int footprintWidth,
            int footprintLength,
            float buildWorkRequired,
            ushort blueprintArchetypeId,
            ushort finishedArchetypeId,
            string ghostPreviewViewPath,
            string blueprintViewPath,
            string finishedViewPath)
        {
            Id = id;
            DisplayName = displayName;
            CostWood = costWood;
            CostStone = costStone;
            FootprintWidth = footprintWidth;
            FootprintLength = footprintLength;
            BuildWorkRequired = buildWorkRequired;
            BlueprintArchetypeId = blueprintArchetypeId;
            FinishedArchetypeId = finishedArchetypeId;
            GhostPreviewViewPath = ghostPreviewViewPath;
            BlueprintViewPath = blueprintViewPath;
            FinishedViewPath = finishedViewPath;
        }
    }
}
