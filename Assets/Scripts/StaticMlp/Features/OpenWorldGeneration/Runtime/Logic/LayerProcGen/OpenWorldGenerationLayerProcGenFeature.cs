using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationLayerProcGenFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            OpenWorldGenerationNetworkEvents.Register();
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(OpenWorldGenerationServerRuntime.CreateDefault(LayerProcGenWorldGenerationService.AcquireShared));
            SW.SetResource(new OpenWorldChunkStreamingState());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerOpenWorldChunkInterestSystem(), GameplaySystemOrder.ServerConnectionGameplay + 30);
            systems.Add(new ServerOpenWorldChunkSnapshotSystem(), GameplaySystemOrder.ServerConnectionGameplay + 40);
            systems.Add(new ServerOpenWorldGenerationRuntimeCleanupSystem(), GameplaySystemOrder.CollectReplication + 90);
        }
    }
}
