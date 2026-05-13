using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Build
{
    public sealed class ClientBuildPreparationScreenStateSystem : ISystem
    {
        public void Update()
        {
            var next = new BuildPreparationScreenState
            {
                PoisonArrowAvailable = true,
                FireFlaskAvailable = true,
                SelectedPrimaryModuleId = BuildModuleCatalog.PoisonArrowModuleId,
            };

            foreach (var player in CW.Query<All<OwnerBuildSelection>>().Entities())
            {
                next.SelectedPrimaryModuleId = player.Read<OwnerBuildSelection>().PrimaryModuleId;
                break;
            }

            if (Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
            {
                next.IsAvailable = ClientProjection.Read<Stage1FlowViewState>(anchor).CanOpenBuildPreparation;

                if (anchor.Has<Projected<Stage1ProgressionState>>())
                {
                    ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
                    next.CanPrepareBoss = progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                                          && !progression.HasFlag(ProgressFlagCatalog.BossUnlockedId)
                                          && progression.HasBossPreparationToken();
                }
            }

            foreach (var bossPreparation in CW.Query<All<BossBuildPreparationState>>().Entities())
            {
                next.IsBossCommitted = bossPreparation.Read<BossBuildPreparationState>().Status == BossBuildPreparationStatus.Committed;
                break;
            }

            next.PoisonArrowSelected = next.SelectedPrimaryModuleId == BuildModuleCatalog.PoisonArrowModuleId;
            next.FireFlaskSelected = next.SelectedPrimaryModuleId == BuildModuleCatalog.FireFlaskModuleId;
            next.CanConfirm = next.IsAvailable && !next.IsBossCommitted;

            CW.SetResource(next);
        }
    }
}
