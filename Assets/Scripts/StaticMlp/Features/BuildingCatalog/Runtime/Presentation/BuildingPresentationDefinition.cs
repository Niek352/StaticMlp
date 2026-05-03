namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingPresentationDefinition
    {
        public readonly BuildingId Id;
        public readonly string GhostPreviewViewPath;
        public readonly string BlueprintViewPath;
        public readonly string FinishedViewPath;

        public BuildingPresentationDefinition(
            BuildingId id,
            string ghostPreviewViewPath,
            string blueprintViewPath,
            string finishedViewPath)
        {
            Id = id;
            GhostPreviewViewPath = ghostPreviewViewPath;
            BlueprintViewPath = blueprintViewPath;
            FinishedViewPath = finishedViewPath;
        }
    }
}
