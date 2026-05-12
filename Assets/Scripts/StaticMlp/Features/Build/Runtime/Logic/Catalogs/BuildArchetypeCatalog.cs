using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Build
{
    public static class BuildArchetypeCatalog
    {
        public static readonly BuildArchetypeId PoisonArcherId = new(1);
        public static readonly BuildArchetypeId FireBomberId = new(2);

        private static readonly BuildArchetypeDefinition[] Definitions =
        {
            new(PoisonArcherId),
            new(FireBomberId)
        };

        public static IReadOnlyList<BuildArchetypeDefinition> All => Definitions;

        public static BuildArchetypeDefinition Get(BuildArchetypeId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(BuildArchetypeDefinition)} for build archetype id {id.Value} in {nameof(BuildArchetypeCatalog)}.");
        }

        public static bool TryGet(BuildArchetypeId id, out BuildArchetypeDefinition definition)
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
