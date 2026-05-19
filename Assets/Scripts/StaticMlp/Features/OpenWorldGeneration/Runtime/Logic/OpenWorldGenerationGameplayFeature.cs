using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationGameplayFeature : GameplayFeature
    {
        public override void RegisterServerResources()
        {
            var generationRuntime = OpenWorldChunkGenerationRuntime.CreateDefault();
            SW.SetResource(generationRuntime);
            SW.SetResource(OpenWorldGenerationServerRuntime.CreateDefault());
            SW.SetResource(new OpenWorldChunkStreamingState());
            SW.SetResource(new OpenWorldServerChunkGeometryRuntime());
            SW.SetResource(new OpenWorldNavMeshSurfaceRuntime());
            SW.SetResource<IHeightSampler>(new OpenWorldHeightSampler(generationRuntime.Seed));
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerOpenWorldChunkInterestSystem(), GameplaySystemOrder.ServerConnectionGameplay + 30);
            systems.Add(new ServerOpenWorldChunkGenerationBridgeSystem(), GameplaySystemOrder.ServerConnectionGameplay + 31);
            systems.Add(new ServerOpenWorldChunkGenerationSystem(), GameplaySystemOrder.ServerConnectionGameplay + 32);
            systems.Add(new ServerOpenWorldChunkGeometryStoreSystem(), GameplaySystemOrder.ServerConnectionGameplay + 36);
            systems.Add(new ServerOpenWorldNavMeshSurfaceSystem(), GameplaySystemOrder.ServerConnectionGameplay + 37);
            systems.Add(new ServerOpenWorldChunkGenerationCompleteSystem(), GameplaySystemOrder.ServerConnectionGameplay + 38);
            systems.Add(new ServerOpenWorldChunkSnapshotSystem(), GameplaySystemOrder.ServerConnectionGameplay + 40);
            //systems.Add(new ServerOpenWorldGenerationRuntimeCleanupSystem(), GameplaySystemOrder.CollectReplication + 90);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            CW.SetResource(CreateClientGenerationRuntime());
            
            systems.Add(new ClientOpenWorldChunkGenerationSystem(), GameplaySystemOrder.ClientPresentation - 40);
        }

        private static OpenWorldChunkGenerationRuntime CreateClientGenerationRuntime()
        {
            if (SW.IsWorldInitialized && SW.HasResource<OpenWorldChunkGenerationRuntime>())
                return SW.GetResource<OpenWorldChunkGenerationRuntime>();

            return OpenWorldChunkGenerationRuntime.CreateDefault();
        }
    }
}
