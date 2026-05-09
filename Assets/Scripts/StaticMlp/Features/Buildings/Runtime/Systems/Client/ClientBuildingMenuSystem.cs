using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientBuildingMenuSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new BuildingMenuState
            {
                SelectionFrame = -1,
            });
        }

        public void Update()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            var inputState = CW.GetResource<ClientInputState>();
            if (inputState.WasPressed(BuildingsInputActions.BuildMenuToggle))
                state.IsOpen = !state.IsOpen;
        }
    }
}
