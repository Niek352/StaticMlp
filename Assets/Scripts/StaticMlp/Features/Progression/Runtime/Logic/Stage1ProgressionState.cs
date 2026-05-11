using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public struct Stage1ProgressionState : IComponent
    {
        public ushort AnchorId;
        public uint AppliedFlagsMask;
        public uint AppliedRewardsMask;

        public Stage1ProgressionState(SettlementAnchorId anchorId, uint appliedFlagsMask)
        {
            AnchorId = anchorId.Value;
            AppliedFlagsMask = appliedFlagsMask;
            AppliedRewardsMask = 0;
        }

        public bool HasFlag(ProgressFlagId flagId)
        {
            return (AppliedFlagsMask & GetFlagBit(flagId)) != 0;
        }

        public void ApplyFlag(ProgressFlagId flagId)
        {
            AppliedFlagsMask |= GetFlagBit(flagId);
        }

        public bool HasAppliedReward(RewardPackageId rewardPackageId)
        {
            return (AppliedRewardsMask & GetRewardBit(rewardPackageId)) != 0;
        }

        public void MarkRewardApplied(RewardPackageId rewardPackageId)
        {
            AppliedRewardsMask |= GetRewardBit(rewardPackageId);
        }

        public static uint GetFlagBit(ProgressFlagId flagId)
        {
            if (flagId.Value == 0 || flagId.Value > 32)
            {
                throw new InvalidOperationException(
                    $"Progress flag id {flagId.Value} is outside supported Stage 1 mask range.");
            }

            return 1u << (flagId.Value - 1);
        }

        private static uint GetRewardBit(RewardPackageId rewardPackageId)
        {
            if (rewardPackageId.Value == 0 || rewardPackageId.Value > 32)
            {
                throw new InvalidOperationException(
                    $"Reward package id {rewardPackageId.Value} is outside supported Stage 1 mask range.");
            }

            return 1u << (rewardPackageId.Value - 1);
        }
    }
}
