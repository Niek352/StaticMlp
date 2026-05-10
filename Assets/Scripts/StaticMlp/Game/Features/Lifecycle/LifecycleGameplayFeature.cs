using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Systems.Server;

namespace StaticMlp.Game.Features.Lifecycle
{
    public sealed class LifecycleGameplayFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerLifeTimeExpireMarkSystem(), GameplaySystemOrder.CollectReplication - 20);
            systems.Add(new ServerDestroyedEntityCleanupSystem(), GameplaySystemOrder.CollectReplication - 10);
        }
    }
}
