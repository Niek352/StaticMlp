using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryMinimalGameplayFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerResourcesInventorySeedSystem(), GameplaySystemOrder.ServerConnectionGameplay + 10);
        }
    }
}
