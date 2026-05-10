namespace StaticMlp.Features.World
{
    public readonly struct RaidDefinition
    {
        public readonly RaidId Id;
        public readonly RegionId SourceRegionId;
        public readonly EncounterProfileId EncounterProfileId;
        public readonly int ThreatValue;

        public RaidDefinition(
            RaidId id,
            RegionId sourceRegionId,
            EncounterProfileId encounterProfileId,
            int threatValue)
        {
            Id = id;
            SourceRegionId = sourceRegionId;
            EncounterProfileId = encounterProfileId;
            ThreatValue = threatValue;
        }
    }
}
