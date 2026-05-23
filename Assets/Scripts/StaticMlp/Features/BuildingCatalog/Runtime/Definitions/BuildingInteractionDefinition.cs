namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingInteractionDefinition
    {
        public readonly BuildingInteractionKind Kind;
        public readonly string DisplayName;
        public readonly bool RequiresCompletedBuilding;
        public readonly byte Priority;

        public BuildingInteractionDefinition(
            BuildingInteractionKind kind,
            string displayName,
            bool requiresCompletedBuilding,
            byte priority = 0)
        {
            Kind = kind;
            DisplayName = displayName;
            RequiresCompletedBuilding = requiresCompletedBuilding;
            Priority = priority;
        }
    }
}
