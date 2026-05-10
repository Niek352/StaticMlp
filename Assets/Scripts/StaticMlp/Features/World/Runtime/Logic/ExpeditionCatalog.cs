using System;
using System.Collections.Generic;

namespace StaticMlp.Features.World
{
    public static class ExpeditionCatalog
    {
        public static readonly ExpeditionId NearbyRaiderCampId = new(1);
        public static readonly ExpeditionId ChiefStandId = new(2);

        private static readonly ExpeditionDefinition[] Definitions =
        {
            new(
                NearbyRaiderCampId,
                RegionCatalog.RaiderFrontierId,
                EncounterProfileCatalog.RaiderSkirmishId,
                threatTier: 1),
            new(
                ChiefStandId,
                RegionCatalog.RaiderFrontierId,
                EncounterProfileCatalog.RaiderChiefId,
                threatTier: 3)
        };

        public static IReadOnlyList<ExpeditionDefinition> All => Definitions;

        public static ExpeditionDefinition Get(ExpeditionId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(ExpeditionDefinition)} for expedition id {id.Value} in {nameof(ExpeditionCatalog)}.");
        }

        public static bool TryGet(ExpeditionId id, out ExpeditionDefinition definition)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id != id)
                    continue;

                definition = Definitions[i];
                return true;
            }

            definition = default;
            return false;
        }
    }
}
