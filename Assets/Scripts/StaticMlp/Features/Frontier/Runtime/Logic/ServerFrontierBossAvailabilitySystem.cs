using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierBossAvailabilitySystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<ProgressionState, BossEncounterState>>().Entities())
            {
                ref readonly var progression = ref anchor.Read<ProgressionState>();
                ref readonly var bossState = ref anchor.Read<BossEncounterState>();
                if (bossState.Status is BossEncounterStatus.Active or BossEncounterStatus.Defeated)
                    continue;

                var targetStatus = progression.HasFlag(ProgressFlagCatalog.BossUnlockedId)
                    ? BossEncounterStatus.Available
                    : BossEncounterStatus.Unavailable;

                if (bossState.Status == targetStatus)
                    continue;

                ref var mutableBossState = ref ReplicationMut.Mut<BossEncounterState>(anchor);
                mutableBossState.Status = targetStatus;
            }
        }
    }
}
