using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierExpeditionResolutionSystem : ISystem
    {
        private const float RAID_DELAY_SECONDS = 5f;

        public void Update()
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression, ActiveExpeditionState, ThreatState, RaidScheduleState>>().Entities())
            {
                ref readonly var activeExpedition = ref anchor.Read<ActiveExpeditionState>();
                if (activeExpedition.Status != ExpeditionActivityStatus.Active)
                    continue;

                var anchorId = anchor.Read<Stage1SettlementProgression>().Anchor;
                if (FrontierEncounterParticipantQuery.HasAnyLivingParticipant(
                        anchorId,
                        FrontierEncounterKind.Expedition,
                        activeExpedition.ExpeditionIdValue))
                {
                    continue;
                }

                ref var mutableExpedition = ref ReplicationMut.Mut<ActiveExpeditionState>(anchor);
                mutableExpedition.Status = ExpeditionActivityStatus.Cleared;

                var raidId = RaidCatalog.RaiderCounterattackId;
                var raid = RaidCatalog.Get(raidId);

                ref var threat = ref ReplicationMut.Mut<ThreatState>(anchor);
                threat.Phase = ThreatPhase.RaidPending;
                threat.ThreatValue = (ushort)raid.ThreatValue;

                ref var schedule = ref ReplicationMut.Mut<RaidScheduleState>(anchor);
                schedule.RaidIdValue = raidId.Value;
                schedule.Status = RaidScheduleStatus.Pending;
                schedule.ActivateAtTick = simulationTime.DeadlineAfter(RAID_DELAY_SECONDS);
            }
        }
    }
}
