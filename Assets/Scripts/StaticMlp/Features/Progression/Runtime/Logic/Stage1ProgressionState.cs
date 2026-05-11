using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public struct Stage1ProgressionState : IComponent, ITrackableChanged
    {
        public ushort AnchorId;
        public uint AppliedFlagsMask;
        public uint AppliedRewardsMask;
        public byte BossPreparationTokens;

        public Stage1ProgressionState(SettlementAnchorId anchorId, uint appliedFlagsMask)
        {
            AnchorId = anchorId.Value;
            AppliedFlagsMask = appliedFlagsMask;
            AppliedRewardsMask = 0;
            BossPreparationTokens = 0;
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

        public void GrantBossPreparationTokens(byte amount)
        {
            BossPreparationTokens += amount;
        }

        public bool HasBossPreparationToken()
        {
            return BossPreparationTokens > 0;
        }

        public bool TrySpendBossPreparationToken()
        {
            if (BossPreparationTokens == 0)
                return false;

            BossPreparationTokens--;
            return true;
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
