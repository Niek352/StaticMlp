using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    public static class CombatNetworkEvents
    {
        private const ushort PASSIVE_AUTO_ATTACK_REQUEST_EVENT_TYPE_ID = 57021;

        public static void Register()
        {
            NetworkEventRegistry.Register<PassiveAutoAttackRequestEvent>(
                PASSIVE_AUTO_ATTACK_REQUEST_EVENT_TYPE_ID,
                NetDelivery.ReliableSequenced,
                WritePassiveAutoAttackRequestEvent,
                TryReadPassiveAutoAttackRequestEvent);
        }

        private static byte[] WritePassiveAutoAttackRequestEvent(in PassiveAutoAttackRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(16);
            writer.WriteUlong(evt.Target.Raw);
            writer.WriteUint(evt.ShotSequence);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static bool TryReadPassiveAutoAttackRequestEvent(byte[] payload, out PassiveAutoAttackRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new PassiveAutoAttackRequestEvent(
                    new EntityGID(reader.ReadUlong()),
                    reader.ReadUint());
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
