using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Progression
{
    public sealed class ClientRewardResultPopupStateSystem : ISystem
    {
        public void Update()
        {
            ref var popupState = ref CW.GetResource<RewardResultPopupState>();
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor)
                || !anchor.Has<Projected<Stage1ProgressionState>>())
            {
                return;
            }

            ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
            popupState.CurrentAppliedRewardsMask = progression.AppliedRewardsMask;

            if (popupState.IsVisible)
                return;

            var newlyAppliedMask = progression.AppliedRewardsMask & ~popupState.LastPresentedRewardsMask;
            if (newlyAppliedMask == 0u)
                return;

            var rewardId = ResolveReward(newlyAppliedMask);
            var rewardDefinition = RewardPackageCatalog.Get(rewardId);
            popupState.RewardPackageId = rewardId;
            popupState.IsVisible = true;
            popupState.GrantedWood = GetGrantedAmount(rewardDefinition, Settlement.ResourceCatalog.WoodId);
            popupState.GrantedStone = GetGrantedAmount(rewardDefinition, Settlement.ResourceCatalog.StoneId);
            popupState.GrantsRecoveredWarCacheFlag = progression.HasFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId);
            popupState.ThreatRaised = popupState.GrantsRecoveredWarCacheFlag;
        }

        private static RewardPackageId ResolveReward(uint newlyAppliedMask)
        {
            for (var i = 0; i < RewardPackageCatalog.All.Count; i++)
            {
                var reward = RewardPackageCatalog.All[i];
                var bit = 1u << (reward.Id.Value - 1);
                if ((newlyAppliedMask & bit) != 0)
                    return reward.Id;
            }

            throw new System.InvalidOperationException($"Unable to resolve Stage 1 reward from mask {newlyAppliedMask}.");
        }

        private static int GetGrantedAmount(RewardPackageDefinition rewardDefinition, Settlement.ResourceId resourceId)
        {
            for (var i = 0; i < rewardDefinition.ResourceGrants.Length; i++)
            {
                var grant = rewardDefinition.ResourceGrants[i];
                if (grant.Id == resourceId)
                    return grant.Amount;
            }

            return 0;
        }
    }
}
