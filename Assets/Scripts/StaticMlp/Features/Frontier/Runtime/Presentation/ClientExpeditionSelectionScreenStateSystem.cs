using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Build;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientExpeditionSelectionScreenStateSystem : ISystem
    {
        public void Update()
        {
            var next = new ExpeditionSelectionScreenState
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId,
                ExpeditionId = ExpeditionCatalog.NearbyRaiderCampId,
                BossId = BossCatalog.RaiderChiefId,
                RewardPackageId = RewardPackageCatalog.RecoveredWarCacheId,
            };

            if (Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
            {
                if (anchor.Has<Projected<ExpeditionAvailabilityState>>())
                {
                    ref readonly var availability = ref ClientProjection.Read<ExpeditionAvailabilityState>(anchor);
                    next.ExpeditionId = availability.ExpeditionId;
                    next.AvailabilityStatus = availability.Status;
                }

                if (anchor.Has<Projected<ActiveExpeditionState>>())
                    next.ActivityStatus = ClientProjection.Read<ActiveExpeditionState>(anchor).Status;

                if (anchor.Has<Projected<ThreatState>>())
                    next.ThreatPhase = ClientProjection.Read<ThreatState>(anchor).Phase;

                if (anchor.Has<Projected<BossEncounterState>>())
                {
                    ref readonly var boss = ref ClientProjection.Read<BossEncounterState>(anchor);
                    next.BossId = boss.BossId;
                    next.BossStatus = boss.Status;
                    next.IsBossEncounterMode = boss.Status == BossEncounterStatus.Available;
                }
            }

            foreach (var player in CW.Query<All<PreparedBuildSnapshot>>().Entities())
            {
                next.PreparedPrimaryModuleId = player.Read<PreparedBuildSnapshot>().PrimaryModuleId;
                break;
            }

            next.CanStart = next.IsBossEncounterMode
                ? next.BossStatus == BossEncounterStatus.Available
                  && next.ActivityStatus == ExpeditionActivityStatus.None
                  && next.ThreatPhase != ThreatPhase.RaidPending
                  && next.ThreatPhase != ThreatPhase.RaidActive
                : next.AvailabilityStatus == ExpeditionAvailabilityStatus.Available
                  && next.ActivityStatus == ExpeditionActivityStatus.None
                  && next.ThreatPhase != ThreatPhase.RaidPending
                  && next.ThreatPhase != ThreatPhase.RaidActive;

            CW.SetResource(next);
        }
    }
}
