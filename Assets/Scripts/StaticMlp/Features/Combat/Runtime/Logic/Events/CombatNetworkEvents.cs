using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Effects;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    public static class CombatNetworkEvents
    {
        private const ushort PASSIVE_AUTO_ATTACK_REQUEST_EVENT_TYPE_ID = 57021;
        private const ushort USE_ABILITY_COMMAND_EVENT_TYPE_ID = 57022;
        private const ushort DAMAGE_NUMBER_EVENT_TYPE_ID = 57023;
        private const ushort DEATH_EVENT_TYPE_ID = 57024;

        public static void Register()
        {
            NetworkEventRegistry.Register<PassiveAutoAttackRequestEvent>(
                PASSIVE_AUTO_ATTACK_REQUEST_EVENT_TYPE_ID,
                NetDelivery.ReliableSequenced,
                WritePassiveAutoAttackRequestEvent,
                TryReadPassiveAutoAttackRequestEvent);
            NetworkEventRegistry.Register<UseAbilityCommand>(
                USE_ABILITY_COMMAND_EVENT_TYPE_ID,
                NetDelivery.ReliableSequenced,
                WriteUseAbilityCommand,
                TryReadUseAbilityCommand);
            NetworkEventRegistry.Register<DamageNumberEvent>(
                DAMAGE_NUMBER_EVENT_TYPE_ID,
                NetDelivery.UnreliableSequenced,
                WriteDamageNumberEvent,
                TryReadDamageNumberEvent);
            NetworkEventRegistry.Register<DeathEvent>(
                DEATH_EVENT_TYPE_ID,
                NetDelivery.ReliableSequenced,
                WriteDeathEvent,
                TryReadDeathEvent);
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

        private static byte[] WriteUseAbilityCommand(in UseAbilityCommand evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(16);
            writer.WriteUshort((ushort)evt.AbilityId);
            writer.WriteUlong(evt.Target.Raw);
            writer.WriteUint(evt.ClientCommandId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static bool TryReadUseAbilityCommand(byte[] payload, out UseAbilityCommand evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new UseAbilityCommand(
                    (CombatAbilityId)reader.ReadUshort(),
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

        private static byte[] WriteDamageNumberEvent(in DamageNumberEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(32);
            writer.WriteUlong(evt.Source.Raw);
            writer.WriteUlong(evt.Target.Raw);
            writer.WriteUint(evt.ClientCommandId);
            writer.WriteFloat(evt.Value);
            writer.WriteByte((byte)evt.DamageType);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static bool TryReadDamageNumberEvent(byte[] payload, out DamageNumberEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new DamageNumberEvent
                {
                    Source = new EntityGID(reader.ReadUlong()),
                    Target = new EntityGID(reader.ReadUlong()),
                    ClientCommandId = reader.ReadUint(),
                    Value = reader.ReadFloat(),
                    DamageType = (DamageType)reader.ReadByte()
                };
                return true;
            }
            catch
            {
                evt = default;
                return false;
            }
        }

        private static byte[] WriteDeathEvent(in DeathEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(24);
            writer.WriteUlong(evt.Source.Raw);
            writer.WriteUlong(evt.Target.Raw);
            writer.WriteUint(evt.ClientCommandId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        private static bool TryReadDeathEvent(byte[] payload, out DeathEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new DeathEvent
                {
                    Source = new EntityGID(reader.ReadUlong()),
                    Target = new EntityGID(reader.ReadUlong()),
                    ClientCommandId = reader.ReadUint(),
                };
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
