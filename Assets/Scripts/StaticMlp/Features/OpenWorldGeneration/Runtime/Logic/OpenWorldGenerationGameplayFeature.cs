using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationGameplayFeature : GameplayFeature
    {
        public override void RegisterServerResources()
        {
            SW.SetResource(OpenWorldChunkGenerationRuntime.CreateDefault());
            SW.SetResource(OpenWorldGenerationServerRuntime.CreateDefault());
            SW.SetResource(new OpenWorldChunkStreamingState());
            SW.SetResource(new OpenWorldServerChunkGeometryRuntime());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerOpenWorldChunkInterestSystem(), GameplaySystemOrder.ServerConnectionGameplay + 30);
            systems.Add(new ServerOpenWorldChunkGenerationBridgeSystem(), GameplaySystemOrder.ServerConnectionGameplay + 31);
            systems.Add(new ServerOpenWorldChunkGenerationSystem(), GameplaySystemOrder.ServerConnectionGameplay + 32);
            systems.Add(new ServerOpenWorldChunkGeometryStoreSystem(), GameplaySystemOrder.ServerConnectionGameplay + 36);
            systems.Add(new ServerOpenWorldChunkGenerationCompleteSystem(), GameplaySystemOrder.ServerConnectionGameplay + 38);
            systems.Add(new ServerOpenWorldChunkSnapshotSystem(), GameplaySystemOrder.ServerConnectionGameplay + 40);
            //systems.Add(new ServerOpenWorldGenerationRuntimeCleanupSystem(), GameplaySystemOrder.CollectReplication + 90);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            CW.SetResource(OpenWorldChunkGenerationRuntime.CreateDefault());
            
            systems.Add(new ClientOpenWorldChunkGenerationSystem(), GameplaySystemOrder.ClientPresentation - 40);
        }
    }
}
