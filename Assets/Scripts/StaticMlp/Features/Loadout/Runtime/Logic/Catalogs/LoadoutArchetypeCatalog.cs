using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Loadout
{
    public static class LoadoutArchetypeCatalog
    {
        public static readonly LoadoutArchetypeId PoisonArcherId = new(1);
        public static readonly LoadoutArchetypeId FireBomberId = new(2);

        private static readonly LoadoutArchetypeDefinition[] Definitions =
        {
            new(PoisonArcherId),
            new(FireBomberId)
        };

        public static IReadOnlyList<LoadoutArchetypeDefinition> All => Definitions;

        public static LoadoutArchetypeDefinition Get(LoadoutArchetypeId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(LoadoutArchetypeDefinition)} for build archetype id {id.Value} in {nameof(LoadoutArchetypeCatalog)}.");
        }

        public static bool TryGet(LoadoutArchetypeId id, out LoadoutArchetypeDefinition definition)
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
