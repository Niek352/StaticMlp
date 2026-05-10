using System;
using System.Collections.Generic;

namespace StaticMlp.Features.World
{
    public static class EncounterProfileCatalog
    {
        public static readonly EncounterProfileId RaiderSkirmishId = new(1);
        public static readonly EncounterProfileId RaiderChiefId = new(2);

        private static readonly EncounterProfileDefinition[] Definitions =
        {
            new(RaiderSkirmishId, threatTier: 1),
            new(RaiderChiefId, threatTier: 3)
        };

        public static IReadOnlyList<EncounterProfileDefinition> All => Definitions;

        public static EncounterProfileDefinition Get(EncounterProfileId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(EncounterProfileDefinition)} for encounter profile id {id.Value} in {nameof(EncounterProfileCatalog)}.");
        }

        public static bool TryGet(EncounterProfileId id, out EncounterProfileDefinition definition)
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
