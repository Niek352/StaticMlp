using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public static class DepositCarriedResourcesToStockpileEventCodec
    {
        public static void Register()
        {
            NetworkEventRegistry.Register<DepositCarriedResourcesToStockpileRequestEvent>(
                DepositCarriedResourcesToStockpileRequestEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                24,
                WriteRequest,
                ReadRequest);
            NetworkEventRegistry.Register<DepositCarriedResourcesToStockpileResultEvent>(
                DepositCarriedResourcesToStockpileResultEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                32,
                WriteResult,
                ReadResult);
        }

        private static void WriteRequest(
            ref NetworkWriter writer,
            in DepositCarriedResourcesToStockpileRequestEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteEntityGid(evt.Building);
        }

        private static DepositCarriedResourcesToStockpileRequestEvent ReadRequest(ref NetworkReader reader)
        {
            return new DepositCarriedResourcesToStockpileRequestEvent
            {
                RequestId = reader.ReadRequestId(),
                Building = reader.ReadEntityGid()
            };
        }

        private static void WriteResult(
            ref NetworkWriter writer,
            in DepositCarriedResourcesToStockpileResultEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteByte((byte)evt.Status);
            writer.WriteEntityGid(evt.Building);
            writer.WriteInt(evt.TransferredAmount);
        }

        private static DepositCarriedResourcesToStockpileResultEvent ReadResult(ref NetworkReader reader)
        {
            return new DepositCarriedResourcesToStockpileResultEvent
            {
                RequestId = reader.ReadRequestId(),
                Status = (RequestStatus)reader.ReadByte(),
                Building = reader.ReadEntityGid(),
                TransferredAmount = reader.ReadInt()
            };
        }
    }
}
