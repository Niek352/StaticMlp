using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementSharedResourcesGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<SettlementSharedResources>();
            ProjectionRegistry.Register<Stage1SettlementProgression>();
        }

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(SettlementNetworkArchetypeIds.ResourceStorage, e =>
            {
                e.Set<SettlementResourceStorageTag>();
            });

            NetArchetypeRegistry.RegisterServer(SettlementNetworkArchetypeIds.ResourceStorage, e =>
            {
                e.Set<SettlementResourceStorageTag>();
            });
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(new SettlementSharedResourcesFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerSettlementSharedResourcesSpawnSystem(), (short)(GameplaySystemOrder.ServerConnectionGameplay - 15));
        }
    }
}
