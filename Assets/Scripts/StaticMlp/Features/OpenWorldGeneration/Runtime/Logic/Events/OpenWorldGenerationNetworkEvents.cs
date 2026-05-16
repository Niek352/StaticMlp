using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public static class OpenWorldGenerationNetworkEvents
    {
        public static void Register()
        {
            NetworkEventRegistry.Register<OpenWorldChunkUnloadEvent>(
                OpenWorldChunkUnloadEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                OpenWorldChunkUnloadEvent.Write,
                OpenWorldChunkUnloadEvent.TryRead);
        }
    }
}
