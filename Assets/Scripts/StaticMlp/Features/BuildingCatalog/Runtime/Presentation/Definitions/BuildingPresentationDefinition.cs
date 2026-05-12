namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingPresentationDefinition
    {
        public readonly BuildingId Id;
        public readonly string DisplayName;
        public readonly string GhostPreviewViewPath;
        public readonly string BlueprintViewPath;
        public readonly string FinishedViewPath;

        public BuildingPresentationDefinition(
            BuildingId id,
            string displayName,
            string ghostPreviewViewPath,
            string blueprintViewPath,
            string finishedViewPath)
        {
            Id = id;
            DisplayName = displayName;
            GhostPreviewViewPath = ghostPreviewViewPath;
            BlueprintViewPath = blueprintViewPath;
            FinishedViewPath = finishedViewPath;
        }
    }
}
