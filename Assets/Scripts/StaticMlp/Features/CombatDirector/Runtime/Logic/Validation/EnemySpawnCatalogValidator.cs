using System;
using System.Collections.Generic;

namespace StaticMlp.Features.CombatDirector
{
    public static class EnemySpawnCatalogValidator
    {
        public static void Validate(IReadOnlyList<EnemySpawnDefinition> definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            var roles = new HashSet<EnemyRole>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];

                if (definition.Role == 0)
                    throw new InvalidOperationException($"Enemy spawn definition at index {i} has zero {nameof(EnemyRole)}.");

                if (!roles.Add(definition.Role))
                    throw new InvalidOperationException($"Duplicate enemy spawn definition for role {definition.Role}.");

                if (definition.BudgetCost <= 0f)
                {
                    throw new InvalidOperationException(
                        $"Enemy spawn definition for role {definition.Role} has non-positive budget cost.");
                }

                if (definition.MinCountPerWave > definition.MaxCountPerWave)
                {
                    throw new InvalidOperationException(
                        $"Enemy spawn definition for role {definition.Role} inverts min/max wave counts.");
                }

                if (definition.MaxCountPerWave <= 0)
                {
                    throw new InvalidOperationException(
                        $"Enemy spawn definition for role {definition.Role} has non-positive max wave count.");
                }
            }
        }
    }
}
