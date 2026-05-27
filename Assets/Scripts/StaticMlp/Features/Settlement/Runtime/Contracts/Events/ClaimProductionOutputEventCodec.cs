using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public static class ClaimProductionOutputEventCodec
    {
        public static void Register()
        {
            NetworkEventRegistry.Register<ClaimProductionOutputRequestEvent>(
                ClaimProductionOutputRequestEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                32,
                WriteRequest,
                ReadRequest);
            NetworkEventRegistry.Register<ClaimProductionOutputResultEvent>(
                ClaimProductionOutputResultEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                40,
                WriteResult,
                ReadResult);
        }

        private static void WriteRequest(
            ref NetworkWriter writer,
            in ClaimProductionOutputRequestEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteEntityGid(evt.Building);
            writer.WriteUshort(evt.ResourceId);
            writer.WriteInt(evt.RequestedAmount);
        }

        private static ClaimProductionOutputRequestEvent ReadRequest(ref NetworkReader reader)
        {
            return new ClaimProductionOutputRequestEvent
            {
                RequestId = reader.ReadRequestId(),
                Building = reader.ReadEntityGid(),
                ResourceId = reader.ReadUshort(),
                RequestedAmount = reader.ReadInt()
            };
        }

        private static void WriteResult(
            ref NetworkWriter writer,
            in ClaimProductionOutputResultEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteByte((byte)evt.Status);
            writer.WriteEntityGid(evt.Building);
            writer.WriteUshort(evt.ResourceId);
            writer.WriteInt(evt.TransferredAmount);
        }

        private static ClaimProductionOutputResultEvent ReadResult(ref NetworkReader reader)
        {
            return new ClaimProductionOutputResultEvent
            {
                RequestId = reader.ReadRequestId(),
                Status = (RequestStatus)reader.ReadByte(),
                Building = reader.ReadEntityGid(),
                ResourceId = reader.ReadUshort(),
                TransferredAmount = reader.ReadInt()
            };
        }
    }
}
