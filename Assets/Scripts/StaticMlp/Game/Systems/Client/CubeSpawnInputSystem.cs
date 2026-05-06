using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;

namespace StaticMlp.Game.Systems.Client
{
    public sealed class CubeSpawnInputSystem : ISystem
    {
        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            if (!inputState.WasPressed(BuiltinInputActions.DebugSpawnCube))
                return;

            var cameraState = CW.GetResource<ClientCameraState>();
            var request = new SpawnPhysicsCubeRequestEvent(cameraState.Yaw);
            CW.SendToServerEvent(in request);
        }
    }
}
