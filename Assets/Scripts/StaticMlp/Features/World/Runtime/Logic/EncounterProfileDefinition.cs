namespace StaticMlp.Features.World
{
    public readonly struct EncounterProfileDefinition
    {
        public readonly EncounterProfileId Id;
        public readonly int ThreatTier;

        public EncounterProfileDefinition(EncounterProfileId id, int threatTier)
        {
            Id = id;
            ThreatTier = threatTier;
        }
    }
}
