using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public struct StartBossEncounterRequestEvent : IEvent
    {
        public const ushort NETWORK_EVENT_ID = 40182;

        public ushort AnchorId;
        public ushort BossId;

        public StartBossEncounterRequestEvent(SettlementAnchorId anchorId, BossId bossId)
        {
            AnchorId = anchorId.Value;
            BossId = bossId.Value;
        }

        public static byte[] Write(in StartBossEncounterRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(4);
            writer.WriteUshort(evt.AnchorId);
            writer.WriteUshort(evt.BossId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out StartBossEncounterRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new StartBossEncounterRequestEvent
                {
                    AnchorId = reader.ReadUshort(),
                    BossId = reader.ReadUshort()
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
