using System;
using System.Collections.Generic;

namespace StaticMlp.Features.World
{
    public static class RaidCatalog
    {
        public static readonly RaidId RaiderCounterattackId = new(1);

        private static readonly RaidDefinition[] Definitions =
        {
            new(
                RaiderCounterattackId,
                RegionCatalog.RaiderFrontierId,
                EncounterProfileCatalog.RaiderSkirmishId,
                threatValue: 2)
        };

        public static IReadOnlyList<RaidDefinition> All => Definitions;

        public static RaidDefinition Get(RaidId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(RaidDefinition)} for raid id {id.Value} in {nameof(RaidCatalog)}.");
        }

        public static bool TryGet(RaidId id, out RaidDefinition definition)
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
