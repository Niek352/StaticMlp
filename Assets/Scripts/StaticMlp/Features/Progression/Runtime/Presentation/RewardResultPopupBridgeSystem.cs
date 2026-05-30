using System;
using StaticMlp.Features.CampFlow;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Progression
{
    public sealed class RewardResultPopupBridgeSystem : ISystem
    {
        public void Update()
        {
            ref readonly var session = ref CW.GetResource<RewardResultPopupSession>();

            var data = new RewardResultPopupViewData();

            if (CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor)
                && anchor.Has<Projected<ProgressionState>>())
            {
                ref readonly var progression = ref ClientProjection.Read<ProgressionState>(anchor);
                var newlyAppliedMask = progression.AppliedRewardsMask & ~session.LastPresentedRewardsMask;
                if (newlyAppliedMask != 0u)
                {
                    var rewardId = ResolveReward(newlyAppliedMask);
                    var rewardDefinition = RewardPackageCatalog.Get(rewardId);
                    data.RewardPackageId = rewardId;
                    data.GrantedWood = GetGrantedAmount(rewardDefinition, Settlement.ResourceCatalog.WoodId);
                    data.GrantedStone = GetGrantedAmount(rewardDefinition, Settlement.ResourceCatalog.StoneId);
                    data.GrantsRecoveredWarCacheFlag = progression.HasFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId);
                    data.ThreatRaised = data.GrantsRecoveredWarCacheFlag;
                }
            }

            foreach (var entity in CW.Query<All<RewardResultPopupViewData>>().Entities())
            {
                ref var viewData = ref entity.Mut<RewardResultPopupViewData>();
                viewData = data;
                return;
            }

            throw new InvalidOperationException($"{nameof(RewardResultPopupViewData)} entity is missing.");
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

            throw new InvalidOperationException($"Unable to resolve Stage 1 reward from mask {newlyAppliedMask}.");
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
