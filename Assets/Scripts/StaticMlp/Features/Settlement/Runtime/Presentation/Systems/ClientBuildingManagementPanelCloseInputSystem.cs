using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientBuildingManagementPanelCloseInputSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, BuildingManagementPanelCloseIntent> _closeIntents;

        public void Init()
        {
            _closeIntents = CW.RegisterEventReceiver<BuildingManagementPanelCloseIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _closeIntents);
        }

        public void Update()
        {
            foreach (var _ in _closeIntents)
                ClosePanel();

            var inputState = CW.GetResource<ClientInputState>();
            if (!inputState.WasPressed(CoreInputActions.Cancel))
                return;

            ClosePanel();
        }

        private static void ClosePanel()
        {
            ref var session = ref CW.GetResource<BuildingPanelSession>();
            if (!session.IsOpen)
                return;

            ref var feedback = ref CW.GetResource<SettlementTransferFeedbackState>();
            feedback.Clear();
            session.Close();
        }
    }
}
