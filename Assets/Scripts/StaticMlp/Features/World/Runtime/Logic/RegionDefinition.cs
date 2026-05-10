namespace StaticMlp.Features.World
{
    public readonly struct RegionDefinition
    {
        public readonly RegionId Id;
        public readonly ExpeditionId[] Expeditions;
        public readonly RaidId[] PossibleRaids;

        public RegionDefinition(RegionId id, ExpeditionId[] expeditions, RaidId[] possibleRaids)
        {
            Id = id;
            Expeditions = expeditions;
            PossibleRaids = possibleRaids;
        }
    }
}
