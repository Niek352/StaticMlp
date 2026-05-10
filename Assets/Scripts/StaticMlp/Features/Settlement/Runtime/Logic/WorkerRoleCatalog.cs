using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class WorkerRoleCatalog
    {
        public static readonly WorkerRoleId CampBuilderId = new(1);

        private static readonly WorkerRoleDefinition[] Definitions =
        {
            new(
                CampBuilderId,
                WorkerJobFlags.DeliverConstructionResources | WorkerJobFlags.BuildConstruction,
                buildSpeedMultiplier: 1f,
                productionSpeedMultiplier: 1f,
                housingCost: 1,
                raidCombatValue: 1)
        };

        public static IReadOnlyList<WorkerRoleDefinition> All => Definitions;

        public static WorkerRoleDefinition Get(WorkerRoleId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(WorkerRoleDefinition)} for worker role id {id.Value} in {nameof(WorkerRoleCatalog)}.");
        }

        public static bool TryGet(WorkerRoleId id, out WorkerRoleDefinition definition)
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
