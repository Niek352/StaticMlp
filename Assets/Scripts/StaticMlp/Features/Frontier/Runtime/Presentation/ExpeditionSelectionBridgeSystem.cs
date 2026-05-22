using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public sealed class ExpeditionSelectionBridgeSystem : ControllerEcsBridgeSystem<ExpeditionSelectionController>
    {
        protected override void SyncPresentation()
        {
            var state = new ExpeditionSelectionScreenState
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
                    state.ExpeditionId = availability.ExpeditionId;
                    state.AvailabilityStatus = availability.Status;
                }

                if (anchor.Has<Projected<ActiveExpeditionState>>())
                    state.ActivityStatus = ClientProjection.Read<ActiveExpeditionState>(anchor).Status;

                if (anchor.Has<Projected<ThreatState>>())
                    state.ThreatPhase = ClientProjection.Read<ThreatState>(anchor).Phase;

                if (anchor.Has<Projected<BossEncounterState>>())
                {
                    ref readonly var boss = ref ClientProjection.Read<BossEncounterState>(anchor);
                    state.BossId = boss.BossId;
                    state.BossStatus = boss.Status;
                    state.IsBossEncounterMode = boss.Status == BossEncounterStatus.Available;
                }
            }

            foreach (var player in CW.Query<All<PreparedLoadoutSnapshot>>().Entities())
            {
                state.PreparedPrimaryModuleId = player.Read<PreparedLoadoutSnapshot>().PrimaryModuleId;
                break;
            }

            state.CanStart = state.IsBossEncounterMode
                ? state.BossStatus == BossEncounterStatus.Available
                  && state.ActivityStatus == ExpeditionActivityStatus.None
                  && state.ThreatPhase != ThreatPhase.RaidPending
                  && state.ThreatPhase != ThreatPhase.RaidActive
                : state.AvailabilityStatus == ExpeditionAvailabilityStatus.Available
                  && state.ActivityStatus == ExpeditionActivityStatus.None
                  && state.ThreatPhase != ThreatPhase.RaidPending
                  && state.ThreatPhase != ThreatPhase.RaidActive;

            Controller.Apply(in state);
        }
    }
}
