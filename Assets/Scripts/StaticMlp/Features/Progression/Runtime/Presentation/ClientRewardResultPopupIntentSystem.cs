using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class ClientRewardResultPopupIntentSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, RewardResultPopupCloseIntent> _closeIntents;

        public void Init()
        {
            _closeIntents = CW.RegisterEventReceiver<RewardResultPopupCloseIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _closeIntents);
        }

        public void Update()
        {
            foreach (var _ in _closeIntents)
                ClosePopup();
        }

        private static void ClosePopup()
        {
            ref var session = ref CW.GetResource<RewardResultPopupSession>();
            session.IsVisible = false;
            session.LastPresentedRewardsMask = session.CurrentAppliedRewardsMask;
        }
    }
}
