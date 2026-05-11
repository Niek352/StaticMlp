using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Frontier
{
    public static class RegionCatalog
    {
        public static readonly RegionId HomeCampId = new(1);
        public static readonly RegionId RaiderFrontierId = new(2);

        private static readonly RegionDefinition[] Definitions =
        {
            new(
                HomeCampId,
                Array.Empty<ExpeditionId>(),
                Array.Empty<RaidId>()),
            new(
                RaiderFrontierId,
                new[]
                {
                    new ExpeditionId(1),
                    new ExpeditionId(2)
                },
                new[]
                {
                    new RaidId(1)
                })
        };

        public static IReadOnlyList<RegionDefinition> All => Definitions;

        public static RegionDefinition Get(RegionId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(RegionDefinition)} for region id {id.Value} in {nameof(RegionCatalog)}.");
        }

        public static bool TryGet(RegionId id, out RegionDefinition definition)
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
