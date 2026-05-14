using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldResourcesGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<OpenWorldResourceNodeState>();
            ProjectionRegistry.Register<OpenWorldResourceNodeTransform>();
        }

        public override void RegisterPrefabs()
        {
            OpenWorldResourcesReplicationRegistration.Register();

            NetArchetypeRegistry.RegisterClient(OpenWorldResourceNetworkArchetypeIds.ResourceNode, e =>
            {
                e.Set<OpenWorldResourceNodeTag>();
            });

            NetArchetypeRegistry.RegisterServer(OpenWorldResourceNetworkArchetypeIds.ResourceNode, e =>
            {
                e.Set<OpenWorldResourceNodeTag>();
            });
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(new OpenWorldResourceNodeFactory());
            SW.SetResource(new OpenWorldResourceNodeDeltaStore());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerOpenWorldResourceNodeSeedSystem(), GameplaySystemOrder.ServerConnectionGameplay + 35);
            systems.Add(new ServerOpenWorldResourceNodeDeltaCaptureSystem(), GameplaySystemOrder.CollectReplication - 30);
        }
    }
}
