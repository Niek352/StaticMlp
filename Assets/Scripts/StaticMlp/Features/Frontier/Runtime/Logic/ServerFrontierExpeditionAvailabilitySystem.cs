using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierExpeditionAvailabilitySystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression, ExpeditionAvailabilityState, ActiveExpeditionState, ThreatState, RaidScheduleState>>().Entities())
            {
                ref readonly var progression = ref anchor.Read<Stage1SettlementProgression>();
                var shouldBeAvailable =
                    progression.Stage == Stage1SettlementProgressStage.BuildPrepared
                    && anchor.Read<ActiveExpeditionState>().Status == ExpeditionActivityStatus.None
                    && anchor.Read<ThreatState>().Phase != ThreatPhase.RaidPending
                    && anchor.Read<ThreatState>().Phase != ThreatPhase.RaidActive
                    && anchor.Read<RaidScheduleState>().Status == RaidScheduleStatus.None;

                ref var availability = ref ReplicationMut.Mut<ExpeditionAvailabilityState>(anchor);
                availability.ExpeditionIdValue = ExpeditionCatalog.NearbyRaiderCampId.Value;
                availability.Status = shouldBeAvailable
                    ? ExpeditionAvailabilityStatus.Available
                    : ExpeditionAvailabilityStatus.Unavailable;
            }
        }
    }
}
