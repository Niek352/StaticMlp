using System;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerRuntimeProfileCatalog
    {
        // Camp flow balancing: health values reflect role fragility (guard/builder tankier; processor most fragile).
        private const float BUILDER_MAX_HEALTH = 100f;
        private const float GATHERER_MAX_HEALTH = 90f;
        private const float HAULER_MAX_HEALTH = 95f;
        private const float PROCESSOR_MAX_HEALTH = 85f;
        private const float GUARD_MAX_HEALTH = 100f;

        private static readonly SettlementWorkerRuntimeProfile[] Profiles =
        {
            new(
                WorkerRoleCatalog.BuilderId,
                SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER,
                SettlementWorkerBehaviorIds.PEACEFUL_BUILDER,
                maxHealth: BUILDER_MAX_HEALTH),
            new(
                WorkerRoleCatalog.GathererId,
                SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER,
                SettlementWorkerBehaviorIds.PEACEFUL_GATHERER,
                maxHealth: GATHERER_MAX_HEALTH),
            new(
                WorkerRoleCatalog.HaulerId,
                SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER,
                SettlementWorkerBehaviorIds.PEACEFUL_HAULER,
                maxHealth: HAULER_MAX_HEALTH),
            new(
                WorkerRoleCatalog.ProcessorId,
                SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER,
                SettlementWorkerBehaviorIds.PEACEFUL_PROCESSOR,
                maxHealth: PROCESSOR_MAX_HEALTH),
            new(
                WorkerRoleCatalog.GuardId,
                SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER,
                SettlementWorkerBehaviorIds.SETTLEMENT_GUARD,
                maxHealth: GUARD_MAX_HEALTH)
        };

        public static SettlementWorkerRuntimeProfile Get(WorkerRoleId roleId)
        {
            if (TryGet(roleId, out var profile))
                return profile;

            throw new InvalidOperationException(
                $"Missing runtime profile for settlement worker role id {roleId.Value}.");
        }

        public static bool TryGet(WorkerRoleId roleId, out SettlementWorkerRuntimeProfile profile)
        {
            for (var i = 0; i < Profiles.Length; i++)
            {
                if (Profiles[i].RoleId != roleId)
                    continue;

                profile = Profiles[i];
                return true;
            }

            profile = default;
            return false;
        }
    }
}
