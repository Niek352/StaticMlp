using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public readonly struct ExpeditionDefinition
    {
        public readonly ExpeditionId Id;
        public readonly RegionId RegionId;
        public readonly EncounterProfileId EncounterProfileId;
        public readonly int ThreatTier;
        public readonly Vector3 EncounterOrigin;

        public ExpeditionDefinition(
            ExpeditionId id,
            RegionId regionId,
            EncounterProfileId encounterProfileId,
            int threatTier,
            Vector3 encounterOrigin)
        {
            Id = id;
            RegionId = regionId;
            EncounterProfileId = encounterProfileId;
            ThreatTier = threatTier;
            EncounterOrigin = encounterOrigin;
        }
    }
}
