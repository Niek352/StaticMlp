namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingNetworkDefinition
    {
        public readonly BuildingId Id;
        public readonly ushort BlueprintArchetypeId;
        public readonly ushort FinishedArchetypeId;

        public BuildingNetworkDefinition(
            BuildingId id,
            ushort blueprintArchetypeId,
            ushort finishedArchetypeId)
        {
            Id = id;
            BlueprintArchetypeId = blueprintArchetypeId;
            FinishedArchetypeId = finishedArchetypeId;
        }
    }
}
