using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Systems.Client
{
    public sealed class CubeSpawnInputSystem : ISystem
    {
        public void Update()
        {
            if (NetworkRuntime.LocalPeerId.Value == 0 || !NetworkInput.SpawnCubeWasPressedProvider())
                return;

            var writer = BinaryPackWriter.CreateFromPool(4);
            writer.WriteFloat(NetworkInput.CameraYawProvider());
            var payload = writer.CopyToBytes();
            writer.Dispose();

            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.EnqueueNetworkEvent(
                new NetworkPeerId(0),
                GameplayEventTypeIds.SpawnPhysicsCubeRequest,
                payload,
                NetDelivery.ReliableSequenced
            );
        }
    }
}
