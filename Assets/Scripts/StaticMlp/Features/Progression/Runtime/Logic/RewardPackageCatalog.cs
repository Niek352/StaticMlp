using System;
using System.Collections.Generic;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public static class RewardPackageCatalog
    {
        public static readonly RewardPackageId RecoveredWarCacheId = new(1);

        private static readonly RewardPackageDefinition[] Definitions =
        {
            new(
                RecoveredWarCacheId,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 20),
                    new ResourceAmount(ResourceCatalog.StoneId, 10)
                },
                new[]
                {
                    ProgressFlagCatalog.RecoveredWarCacheAppliedId
                },
                bossPreparationTokenGrants: 1)
        };

        public static IReadOnlyList<RewardPackageDefinition> All => Definitions;

        public static RewardPackageDefinition Get(RewardPackageId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(RewardPackageDefinition)} for reward package id {id.Value} in {nameof(RewardPackageCatalog)}.");
        }

        public static bool TryGet(RewardPackageId id, out RewardPackageDefinition definition)
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
