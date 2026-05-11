using System;
using System.Collections.Generic;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public static class EncounterProfileCatalog
    {
        public static readonly EncounterProfileId RaiderSkirmishId = new(1);
        public static readonly EncounterProfileId RaiderChiefId = new(2);

        private static readonly EncounterProfileDefinition[] Definitions =
        {
            new(
                RaiderSkirmishId,
                threatTier: 1,
                hostileSpawns: new[]
                {
                    new EncounterHostileSpawnDefinition(new Vector3(-2f, 0f, 0f), behaviorId: 1, health01: 1f, hunger: 0.05f, fear: 0.1f),
                    new EncounterHostileSpawnDefinition(new Vector3(2f, 0f, 0f), behaviorId: 1, health01: 1f, hunger: 0.1f, fear: 0.1f),
                    new EncounterHostileSpawnDefinition(new Vector3(0f, 0f, 2.5f), behaviorId: 1, health01: 0.85f, hunger: 0.15f, fear: 0.2f)
                }),
            new(
                RaiderChiefId,
                threatTier: 3,
                hostileSpawns: new[]
                {
                    new EncounterHostileSpawnDefinition(new Vector3(0f, 0f, 0f), behaviorId: 1, health01: 1.5f, hunger: 0.1f, fear: 0.05f),
                    new EncounterHostileSpawnDefinition(new Vector3(-2.5f, 0f, -1.5f), behaviorId: 1, health01: 1f, hunger: 0.1f, fear: 0.1f),
                    new EncounterHostileSpawnDefinition(new Vector3(2.5f, 0f, -1.5f), behaviorId: 1, health01: 1f, hunger: 0.1f, fear: 0.1f)
                })
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
