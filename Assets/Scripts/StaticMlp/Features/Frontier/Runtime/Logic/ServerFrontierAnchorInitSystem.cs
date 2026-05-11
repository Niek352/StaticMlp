using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierAnchorInitSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression>>().Entities())
            {
                if (!anchor.Has<ExpeditionAvailabilityState>())
                {
                    anchor.Set(new ExpeditionAvailabilityState
                    {
                        ExpeditionIdValue = ExpeditionCatalog.NearbyRaiderCampId.Value,
                        Status = ExpeditionAvailabilityStatus.Unavailable
                    });
                }

                if (!anchor.Has<ActiveExpeditionState>())
                {
                    anchor.Set(new ActiveExpeditionState
                    {
                        Status = ExpeditionActivityStatus.None
                    });
                }

                if (!anchor.Has<ThreatState>())
                {
                    anchor.Set(new ThreatState
                    {
                        Phase = ThreatPhase.Calm
                    });
                }

                if (!anchor.Has<RaidScheduleState>())
                {
                    anchor.Set(new RaidScheduleState
                    {
                        Status = RaidScheduleStatus.None
                    });
                }
            }
        }
    }
}
