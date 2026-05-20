namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingInteractionDefinition
    {
        public readonly BuildingInteractionKind Kind;
        public readonly bool RequiresCompletedBuilding;

        public BuildingInteractionDefinition(
            BuildingInteractionKind kind,
            bool requiresCompletedBuilding)
        {
            Kind = kind;
            RequiresCompletedBuilding = requiresCompletedBuilding;
        }
    }
}
