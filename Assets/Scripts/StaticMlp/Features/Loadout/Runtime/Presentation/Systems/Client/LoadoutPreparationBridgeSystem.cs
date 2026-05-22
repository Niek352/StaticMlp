using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Loadout
{
    public sealed class LoadoutPreparationBridgeSystem : ControllerEcsBridgeSystem<LoadoutPreparationController>
    {
        protected override void SyncPresentation()
        {
            var state = new LoadoutPreparationScreenState
            {
                PoisonArrowAvailable = true,
                FireFlaskAvailable = true,
                SelectedPrimaryModuleId = LoadoutModuleCatalog.PoisonArrowModuleId,
            };

            foreach (var player in CW.Query<All<OwnerLoadoutSelection>>().Entities())
            {
                state.SelectedPrimaryModuleId = player.Read<OwnerLoadoutSelection>().PrimaryModuleId;
                break;
            }

            if (Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
            {
                state.IsAvailable = ClientProjection.Read<Stage1FlowViewState>(anchor).CanOpenLoadoutPreparation;

                if (anchor.Has<Projected<Stage1ProgressionState>>())
                {
                    ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
                    state.CanPrepareBoss = progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                                          && !progression.HasFlag(ProgressFlagCatalog.BossUnlockedId)
                                          && progression.HasBossPreparationToken();
                }
            }

            foreach (var bossPreparation in CW.Query<All<BossLoadoutPreparationState>>().Entities())
            {
                state.IsBossCommitted = bossPreparation.Read<BossLoadoutPreparationState>().Status == BossLoadoutPreparationStatus.Committed;
                break;
            }

            state.PoisonArrowSelected = state.SelectedPrimaryModuleId == LoadoutModuleCatalog.PoisonArrowModuleId;
            state.FireFlaskSelected = state.SelectedPrimaryModuleId == LoadoutModuleCatalog.FireFlaskModuleId;
            state.CanConfirm = state.IsAvailable && !state.IsBossCommitted;

            Controller.Apply(in state);
        }
    }
}
