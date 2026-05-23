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
            if (!CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor)
                || !anchor.Has<Projected<ProgressionState>>())
            {
                return;
            }

            ref readonly var progression = ref ClientProjection.Read<ProgressionState>(anchor);
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
