using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationPresentationFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientOpenWorldChunkUnloadSystem(), GameplaySystemOrder.ClientApplyNetworkState + 45);
            systems.Add(new ClientOpenWorldTerrainStreamingSystem(), GameplaySystemOrder.ClientPresentation - 30);
            systems.Add(new ClientOpenWorldTerrainApplySystem(), GameplaySystemOrder.ClientPresentation - 25);
        }
    }
}
