using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Progression
{
    public sealed class ServerStage1RewardApplicationSystem : ISystem
    {
        private EventReceiver<ServerWT, ExpeditionRewardGrantedEvent> _rewards;

        public void Init()
        {
            _rewards = SW.RegisterEventReceiver<ExpeditionRewardGrantedEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _rewards);
        }

        public void Update()
        {
            foreach (var evt in _rewards)
                Handle(in evt.Value);
        }

        private static void Handle(in ExpeditionRewardGrantedEvent evt)
        {
            var anchor = Stage1SettlementProgressionQuery.GetServerAnchor(evt.AnchorId);
            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            if (progression.HasAppliedReward(evt.RewardPackageId))
                return;

            var reward = RewardPackageCatalog.Get(evt.RewardPackageId);
            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();

            for (var i = 0; i < reward.ResourceGrants.Length; i++)
            {
                var grant = reward.ResourceGrants[i];
                SettlementSharedResourcesAccess.Add(storageEntity, grant.Id, grant.Amount);
            }

            progression.MarkRewardApplied(evt.RewardPackageId);
            progression.GrantBossPreparationTokens(reward.BossPreparationTokenGrants);
            for (var i = 0; i < reward.ProgressFlagsGranted.Length; i++)
            {
                var flagId = reward.ProgressFlagsGranted[i];
                if (progression.HasFlag(flagId))
                    continue;

                progression.ApplyFlag(flagId);
                SW.SendEvent(new ProgressFlagAppliedEvent(evt.AnchorId, flagId));
            }
        }

    }
}
