using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class WorkerRoleCatalog
    {
        public static readonly WorkerRoleId BuilderId = new(1);
        public static readonly WorkerRoleId CampBuilderId = BuilderId;
        public static readonly WorkerRoleId GathererId = new(2);
        public static readonly WorkerRoleId HaulerId = new(3);
        public static readonly WorkerRoleId ProcessorId = new(4);
        public static readonly WorkerRoleId GuardId = new(5);

        private static readonly WorkerRoleDefinition[] Definitions =
        {
            new(
                BuilderId,
                WorkerJobFlags.DeliverConstructionResources
                | WorkerJobFlags.BuildConstruction
                | WorkerJobFlags.MaintainBuildings,
                buildSpeedMultiplier: 1f,
                productionSpeedMultiplier: 1f,
                housingCost: 1,
                raidCombatValue: 1),
            new(
                GathererId,
                WorkerJobFlags.GatherResources,
                buildSpeedMultiplier: 0.75f,
                productionSpeedMultiplier: 1f,
                housingCost: 1,
                raidCombatValue: 1),
            new(
                HaulerId,
                WorkerJobFlags.HaulResources,
                buildSpeedMultiplier: 0.75f,
                productionSpeedMultiplier: 1f,
                housingCost: 1,
                raidCombatValue: 1),
            new(
                ProcessorId,
                WorkerJobFlags.ProcessRecipe,
                buildSpeedMultiplier: 0.75f,
                productionSpeedMultiplier: 1.1f,
                housingCost: 1,
                raidCombatValue: 1),
            new(
                GuardId,
                WorkerJobFlags.GuardPost,
                buildSpeedMultiplier: 0.5f,
                productionSpeedMultiplier: 0.75f,
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
