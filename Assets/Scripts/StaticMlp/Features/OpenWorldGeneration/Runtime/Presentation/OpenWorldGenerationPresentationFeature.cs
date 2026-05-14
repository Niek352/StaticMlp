using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationPresentationFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientOpenWorldTerrainStreamingSystem(), GameplaySystemOrder.ClientPresentation - 30);
        }
    }
}
