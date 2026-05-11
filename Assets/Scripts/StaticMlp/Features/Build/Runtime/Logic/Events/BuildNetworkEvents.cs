using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Build
{
    public static class BuildNetworkEvents
    {
        private const ushort PREPARE_BUILD_COMMAND_EVENT_TYPE_ID = 58021;

        public static void Register()
        {
            NetworkEventRegistry.Register<PrepareBuildCommand>(
                PREPARE_BUILD_COMMAND_EVENT_TYPE_ID,
                NetDelivery.ReliableSequenced,
                WritePrepareBuildCommand,
                TryReadPrepareBuildCommand);
        }

        private static byte[] WritePrepareBuildCommand(in PrepareBuildCommand evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(2);
            writer.WriteUshort(evt.PrimaryModuleId.Value);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static bool TryReadPrepareBuildCommand(byte[] payload, out PrepareBuildCommand evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new PrepareBuildCommand(new BuildModuleId(reader.ReadUshort()));
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
