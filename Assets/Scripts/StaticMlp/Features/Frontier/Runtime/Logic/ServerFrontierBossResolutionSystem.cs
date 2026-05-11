using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierBossResolutionSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression, BossEncounterState>>().Entities())
            {
                ref readonly var bossState = ref anchor.Read<BossEncounterState>();
                if (bossState.Status != BossEncounterStatus.Active)
                    continue;

                var anchorId = anchor.Read<Stage1SettlementProgression>().Anchor;
                if (FrontierEncounterParticipantQuery.HasAnyLivingParticipant(
                        anchorId,
                        FrontierEncounterKind.Boss,
                        bossState.BossIdValue))
                {
                    continue;
                }

                ref var mutableBossState = ref ReplicationMut.Mut<BossEncounterState>(anchor);
                mutableBossState.Status = BossEncounterStatus.Defeated;
                SW.SendEvent(new VerticalSliceCompleteEvent(anchorId));
            }
        }
    }
}
