using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationLayerProcGenFeature : GameplayFeature
    {
        public override void RegisterServerResources()
        {
            SW.SetResource(OpenWorldGenerationServerRuntime.CreateDefault(LayerProcGenWorldGenerationService.AcquireShared));
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerOpenWorldGenerationRuntimeCleanupSystem(), GameplaySystemOrder.CollectReplication + 90);
        }
    }
}
