using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    public static class AiBotNetworkEvents
    {
        private const ushort COMMAND_BOT_EVENT_TYPE_ID = 56001;

        public static void Register()
        {
            NetworkEventRegistry.Register<CommandBotEvent>(
                COMMAND_BOT_EVENT_TYPE_ID,
                NetDelivery.ReliableSequenced,
                WriteCommandBotEvent,
                TryReadCommandBotEvent);
        }

        private static byte[] WriteCommandBotEvent(in CommandBotEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(24);
            writer.WriteUlong(evt.Bot.Raw);
            writer.WriteUshort(evt.CommandType);
            writer.WriteUlong(evt.Target.Raw);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static bool TryReadCommandBotEvent(byte[] payload, out CommandBotEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new CommandBotEvent(
                    new EntityGID(reader.ReadUlong()),
                    reader.ReadUshort(),
                    new EntityGID(reader.ReadUlong()));
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
