using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Loadout
{
    public sealed class ClientLoadoutPreparationScreenStateSystem : ISystem
    {
        public void Update()
        {
            var next = new LoadoutPreparationScreenState
            {
                PoisonArrowAvailable = true,
                FireFlaskAvailable = true,
                SelectedPrimaryModuleId = LoadoutModuleCatalog.PoisonArrowModuleId,
            };

            foreach (var player in CW.Query<All<OwnerLoadoutSelection>>().Entities())
            {
                next.SelectedPrimaryModuleId = player.Read<OwnerLoadoutSelection>().PrimaryModuleId;
                break;
            }

            if (Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
            {
                next.IsAvailable = ClientProjection.Read<Stage1FlowViewState>(anchor).CanOpenLoadoutPreparation;

                if (anchor.Has<Projected<Stage1ProgressionState>>())
                {
                    ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
                    next.CanPrepareBoss = progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                                          && !progression.HasFlag(ProgressFlagCatalog.BossUnlockedId)
                                          && progression.HasBossPreparationToken();
                }
            }

            foreach (var bossPreparation in CW.Query<All<BossLoadoutPreparationState>>().Entities())
            {
                next.IsBossCommitted = bossPreparation.Read<BossLoadoutPreparationState>().Status == BossLoadoutPreparationStatus.Committed;
                break;
            }

            next.PoisonArrowSelected = next.SelectedPrimaryModuleId == LoadoutModuleCatalog.PoisonArrowModuleId;
            next.FireFlaskSelected = next.SelectedPrimaryModuleId == LoadoutModuleCatalog.FireFlaskModuleId;
            next.CanConfirm = next.IsAvailable && !next.IsBossCommitted;

            CW.SetResource(next);
        }
    }
}
