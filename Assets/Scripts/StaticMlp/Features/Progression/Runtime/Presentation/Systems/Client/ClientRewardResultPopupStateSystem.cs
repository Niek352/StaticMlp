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
            ref var session = ref CW.GetResource<RewardResultPopupSession>();
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor)
                || !anchor.Has<Projected<Stage1ProgressionState>>())
            {
                return;
            }

            ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
            session.CurrentAppliedRewardsMask = progression.AppliedRewardsMask;

            if (session.IsVisible)
                return;

            var newlyAppliedMask = progression.AppliedRewardsMask & ~session.LastPresentedRewardsMask;
            if (newlyAppliedMask == 0u)
                return;

            session.IsVisible = true;
        }
    }
}
