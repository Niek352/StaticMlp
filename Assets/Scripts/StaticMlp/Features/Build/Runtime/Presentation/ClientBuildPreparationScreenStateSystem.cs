using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

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

            foreach (var anchor in CW.Query<All<Stage1SettlementProgression>>().Entities())
            {
                next.IsAvailable = anchor.Read<Stage1SettlementProgression>().Stage >= Stage1SettlementProgressStage.CampRepaired;
                break;
            }

            foreach (var anchor in CW.Query<All<BossBuildPreparationState>>().Entities())
            {
                next.IsBossCommitted = anchor.Read<BossBuildPreparationState>().Status == BossBuildPreparationStatus.Committed;
                break;
            }

            next.PoisonArrowSelected = next.SelectedPrimaryModuleId == BuildModuleCatalog.PoisonArrowModuleId;
            next.FireFlaskSelected = next.SelectedPrimaryModuleId == BuildModuleCatalog.FireFlaskModuleId;
            next.CanConfirm = next.IsAvailable && !next.IsBossCommitted;

            CW.SetResource(next);
        }
    }
}
