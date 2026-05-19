using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EnemySpawnCatalog : IResource
    {
        private readonly EnemySpawnDefinition[] _definitions;

        public EnemySpawnCatalog(EnemySpawnDefinition[] definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            EnemySpawnCatalogValidator.Validate(definitions);
            _definitions = definitions;
        }

        public IReadOnlyList<EnemySpawnDefinition> All => _definitions;

        public EnemySpawnDefinition Get(EnemyRole role)
        {
            for (var i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i].Role != role)
                    continue;

                return _definitions[i];
            }

            throw new InvalidOperationException(
                $"Missing {nameof(EnemySpawnDefinition)} for enemy role {(byte)role} in {nameof(EnemySpawnCatalog)}.");
        }

        public static EnemySpawnCatalog CreateDefault()
        {
            return new EnemySpawnCatalog(new[]
            {
                new EnemySpawnDefinition(EnemyRole.Swarmer, budgetCost: 1f, minCountPerWave: 6, maxCountPerWave: 18),
                new EnemySpawnDefinition(EnemyRole.Marker, budgetCost: 4f, minCountPerWave: 1, maxCountPerWave: 1),
                new EnemySpawnDefinition(EnemyRole.AnchorElite, budgetCost: 10f, minCountPerWave: 1, maxCountPerWave: 1)
            });
        }
    }
}
