using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public struct StartExpeditionRequestEvent : IEvent
    {
        public const ushort NETWORK_EVENT_ID = 40181;

        public ushort AnchorId;
        public ushort ExpeditionId;

        public StartExpeditionRequestEvent(SettlementAnchorId anchorId, ExpeditionId expeditionId)
        {
            AnchorId = anchorId.Value;
            ExpeditionId = expeditionId.Value;
        }

        public static byte[] Write(in StartExpeditionRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(8);
            writer.WriteUshort(evt.AnchorId);
            writer.WriteUshort(evt.ExpeditionId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out StartExpeditionRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new StartExpeditionRequestEvent
                {
                    AnchorId = reader.ReadUshort(),
                    ExpeditionId = reader.ReadUshort()
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
