using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.CampFlow;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Loadout
{
    public sealed class ClientLoadoutPreparationViewDataSystem : ISystem
    {
        public void Update()
        {
            var selectedPrimaryModuleId = LoadoutModuleCatalog.PoisonArrowModuleId;
            foreach (var player in CW.Query<All<OwnerLoadoutSelection>>().Entities())
            {
                selectedPrimaryModuleId = player.Read<OwnerLoadoutSelection>().PrimaryModuleId;
                break;
            }

            var isAvailable = false;
            var canPrepareBoss = false;
            if (CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
            {
                isAvailable = ClientProjection.Read<CampFlowViewState>(anchor).CanOpenLoadoutPreparation;

                if (anchor.Has<Projected<ProgressionState>>())
                {
                    ref readonly var progression = ref ClientProjection.Read<ProgressionState>(anchor);
                    canPrepareBoss = progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                                     && !progression.HasFlag(ProgressFlagCatalog.BossUnlockedId)
                                     && progression.HasBossPreparationToken();
                }
            }

            var isBossCommitted = false;
            foreach (var bossPreparation in CW.Query<All<BossLoadoutPreparationState>>().Entities())
            {
                isBossCommitted = bossPreparation.Read<BossLoadoutPreparationState>().Status == BossLoadoutPreparationStatus.Committed;
                break;
            }

            foreach (var entity in CW.Query<All<LoadoutPreparationViewData>>().Entities())
            {
                ref var data = ref entity.Mut<LoadoutPreparationViewData>();
                data.IsAvailable = isAvailable;
                data.CanPrepareBoss = canPrepareBoss;
                data.IsBossCommitted = isBossCommitted;
                data.SelectedPrimaryModuleId = selectedPrimaryModuleId;
                return;
            }

            throw new InvalidOperationException($"{nameof(LoadoutPreparationViewData)} entity is missing.");
        }
    }
}
