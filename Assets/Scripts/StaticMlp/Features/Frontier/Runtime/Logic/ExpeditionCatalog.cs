using System;
using System.Collections.Generic;
using UnityEngine;

namespace StaticMlp.Features.Frontier
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
                threatTier: 1,
                encounterOrigin: new Vector3(16f, 0f, 18f)),
            new(
                ChiefStandId,
                RegionCatalog.RaiderFrontierId,
                EncounterProfileCatalog.RaiderChiefId,
                threatTier: 3,
                encounterOrigin: new Vector3(24f, 0f, 22f))
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
