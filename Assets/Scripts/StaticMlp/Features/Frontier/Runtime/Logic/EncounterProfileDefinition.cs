namespace StaticMlp.Features.Frontier
{
    public readonly struct EncounterProfileDefinition
    {
        public readonly EncounterProfileId Id;
        public readonly int ThreatTier;
        public readonly EncounterHostileSpawnDefinition[] HostileSpawns;

        public EncounterProfileDefinition(
            EncounterProfileId id,
            int threatTier,
            EncounterHostileSpawnDefinition[] hostileSpawns)
        {
            Id = id;
            ThreatTier = threatTier;
            HostileSpawns = hostileSpawns;
        }
    }
}
