using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Systems
{
    public static class SpawnPhysicsCubeRequestEventCodec
    {
        public static void Register()
        {
            NetworkEventRegistry.Register<SpawnPhysicsCubeRequestEvent>(
                GameplayEventTypeIds.SpawnPhysicsCubeRequest,
                NetDelivery.ReliableSequenced,
                Write,
                TryRead);
        }

        public static byte[] Write(in SpawnPhysicsCubeRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(4);
            writer.WriteFloat(evt.CameraYaw);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out SpawnPhysicsCubeRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new SpawnPhysicsCubeRequestEvent(reader.ReadFloat());
                return true;
            }
            catch
            {
                evt = default;
                return false;
            }
        }
    }
}
