using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public readonly struct BossDefinition
    {
        public readonly BossId Id;
        public readonly RegionId RegionId;
        public readonly EncounterProfileId EncounterProfileId;
        public readonly Vector3 EncounterOrigin;

        public BossDefinition(
            BossId id,
            RegionId regionId,
            EncounterProfileId encounterProfileId,
            Vector3 encounterOrigin)
        {
            Id = id;
            RegionId = regionId;
            EncounterProfileId = encounterProfileId;
            EncounterOrigin = encounterOrigin;
        }
    }
}
