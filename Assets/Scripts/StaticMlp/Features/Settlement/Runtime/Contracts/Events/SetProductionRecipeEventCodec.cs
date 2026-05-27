using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public static class SetProductionRecipeEventCodec
    {
        public static void Register()
        {
            NetworkEventRegistry.Register<SetProductionRecipeRequestEvent>(
                SetProductionRecipeRequestEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                32,
                WriteRequest,
                ReadRequest);
            NetworkEventRegistry.Register<SetProductionRecipeResultEvent>(
                SetProductionRecipeResultEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                32,
                WriteResult,
                ReadResult);
        }

        private static void WriteRequest(
            ref NetworkWriter writer,
            in SetProductionRecipeRequestEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteEntityGid(evt.Building);
            writer.WriteUshort(evt.RecipeId);
        }

        private static SetProductionRecipeRequestEvent ReadRequest(ref NetworkReader reader)
        {
            return new SetProductionRecipeRequestEvent
            {
                RequestId = reader.ReadRequestId(),
                Building = reader.ReadEntityGid(),
                RecipeId = reader.ReadUshort()
            };
        }

        private static void WriteResult(
            ref NetworkWriter writer,
            in SetProductionRecipeResultEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteByte((byte)evt.Status);
            writer.WriteEntityGid(evt.Building);
            writer.WriteUshort(evt.ActiveRecipeId);
        }

        private static SetProductionRecipeResultEvent ReadResult(ref NetworkReader reader)
        {
            return new SetProductionRecipeResultEvent
            {
                RequestId = reader.ReadRequestId(),
                Status = (RequestStatus)reader.ReadByte(),
                Building = reader.ReadEntityGid(),
                ActiveRecipeId = reader.ReadUshort()
            };
        }
    }
}
