namespace StaticMlp.Features.World
{
    public readonly struct ExpeditionDefinition
    {
        public readonly ExpeditionId Id;
        public readonly RegionId RegionId;
        public readonly EncounterProfileId EncounterProfileId;
        public readonly int ThreatTier;

        public ExpeditionDefinition(
            ExpeditionId id,
            RegionId regionId,
            EncounterProfileId encounterProfileId,
            int threatTier)
        {
            Id = id;
            RegionId = regionId;
            EncounterProfileId = encounterProfileId;
            ThreatTier = threatTier;
        }
    }
}
