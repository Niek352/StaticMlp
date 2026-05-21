using System;
using System.Collections.Generic;
using StaticMlp.Features.Npc;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerNpcProfileCatalog
    {
        private static readonly Dictionary<WorkerRoleId, NpcDefinitionId> Mappings = new()
        {
            { WorkerRoleCatalog.BuilderId, NpcDefinitionCatalog.SeededBuilderId },
            { WorkerRoleCatalog.GathererId, NpcDefinitionCatalog.SeededGathererId },
            { WorkerRoleCatalog.HaulerId, NpcDefinitionCatalog.SeededHaulerId },
            { WorkerRoleCatalog.ProcessorId, NpcDefinitionCatalog.SeededProcessorId },
            { WorkerRoleCatalog.GuardId, NpcDefinitionCatalog.SeededGuardId }
        };

        public static NpcDefinitionId Get(WorkerRoleId roleId)
        {
            if (Mappings.TryGetValue(roleId, out var definitionId))
                return definitionId;

            throw new InvalidOperationException(
                $"Missing NPC profile mapping for worker role id {roleId.Value} in {nameof(SettlementWorkerNpcProfileCatalog)}.");
        }

        public static bool TryGet(WorkerRoleId roleId, out NpcDefinitionId definitionId)
        {
            return Mappings.TryGetValue(roleId, out definitionId);
        }
    }
}
