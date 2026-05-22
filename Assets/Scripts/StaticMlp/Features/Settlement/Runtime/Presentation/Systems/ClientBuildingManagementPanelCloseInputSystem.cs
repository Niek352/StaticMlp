using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientBuildingManagementPanelCloseInputSystem : ISystem
    {
        public void Update()
        {
            ref var session = ref CW.GetResource<BuildingPanelSession>();
            if (!session.IsOpen)
                return;

            var inputState = CW.GetResource<ClientInputState>();
            if (!inputState.WasPressed(CoreInputActions.Cancel))
                return;

            session.Close();
        }
    }
}
