using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Progression
{
    public static class ProgressFlagCatalog
    {
        public static readonly ProgressFlagId CampRepairedId = new(1);
        public static readonly ProgressFlagId RecoveredWarCacheAppliedId = new(2);
        public static readonly ProgressFlagId CounterattackDefendedId = new(3);
        public static readonly ProgressFlagId BossUnlockedId = new(4);

        private static readonly ProgressFlagDefinition[] Definitions =
        {
            new(CampRepairedId),
            new(RecoveredWarCacheAppliedId),
            new(CounterattackDefendedId),
            new(BossUnlockedId)
        };

        public static IReadOnlyList<ProgressFlagDefinition> All => Definitions;

        public static ProgressFlagDefinition Get(ProgressFlagId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(ProgressFlagDefinition)} for progress flag id {id.Value} in {nameof(ProgressFlagCatalog)}.");
        }

        public static bool TryGet(ProgressFlagId id, out ProgressFlagDefinition definition)
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
