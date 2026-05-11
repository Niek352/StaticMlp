using System;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerRuntimeProfileCatalog
    {
        private static readonly SettlementWorkerRuntimeProfile[] Profiles =
        {
            new(
                WorkerRoleCatalog.CampBuilderId,
                SettlementWorkerNetworkArchetypeIds.CAMP_BUILDER_WORKER,
                SettlementWorkerBehaviorIds.PEACEFUL_BUILDER,
                maxHealth: 100f)
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
