using System;
using System.Collections.Generic;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public static class BossCatalog
    {
        public static readonly BossId RaiderChiefId = new(1);

        private static readonly BossDefinition[] Definitions =
        {
            new(
                RaiderChiefId,
                RegionCatalog.RaiderFrontierId,
                EncounterProfileCatalog.RaiderChiefId,
                new Vector3(24f, 0f, 22f))
        };

        public static IReadOnlyList<BossDefinition> All => Definitions;

        public static BossDefinition Get(BossId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(BossDefinition)} for boss id {id.Value} in {nameof(BossCatalog)}.");
        }

        public static bool TryGet(BossId id, out BossDefinition definition)
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
