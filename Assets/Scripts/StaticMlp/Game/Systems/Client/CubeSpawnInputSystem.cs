using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;

namespace StaticMlp.Game.Systems.Client
{
    public sealed class CubeSpawnInputSystem : ISystem
    {
        public void Update()
        {
            if (!NetworkInput.SpawnCubeWasPressedProvider())
                return;

            var request = new SpawnPhysicsCubeRequestEvent(NetworkInput.CameraYawProvider());
            CW.SendToServerEvent(in request);
        }
    }
}
