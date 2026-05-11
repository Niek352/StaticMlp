using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierRaidResolutionSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression, RaidScheduleState, ThreatState>>().Entities())
            {
                ref readonly var raidSchedule = ref anchor.Read<RaidScheduleState>();
                if (raidSchedule.Status != RaidScheduleStatus.Active)
                    continue;

                var anchorId = anchor.Read<Stage1SettlementProgression>().Anchor;
                if (FrontierEncounterParticipantQuery.HasAnyLivingParticipant(
                        anchorId,
                        FrontierEncounterKind.Raid,
                        raidSchedule.RaidIdValue))
                {
                    continue;
                }

                ref var mutableSchedule = ref ReplicationMut.Mut<RaidScheduleState>(anchor);
                mutableSchedule.RaidIdValue = 0;
                mutableSchedule.Status = RaidScheduleStatus.None;
                mutableSchedule.ActivateAtTick = 0;

                ref var mutableThreat = ref ReplicationMut.Mut<ThreatState>(anchor);
                mutableThreat.Phase = ThreatPhase.Calm;
                mutableThreat.ThreatValue = 0;

                SW.SendEvent(new RaidDefenseResolvedEvent(anchorId));
            }
        }
    }
}
