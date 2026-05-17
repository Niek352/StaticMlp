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
            OpenWorldChunkOverlayEventCodec.Register();

            if (OpenWorldResourcesCompatibility.UseLegacyReplicatedResourceNodes)
            {
                ProjectionRegistry.Register<OpenWorldResourceNodeState>();
                ProjectionRegistry.Register<OpenWorldResourceNodeTransform>();
            }
        }

        public override void RegisterPrefabs()
        {
            if (!OpenWorldResourcesCompatibility.UseLegacyReplicatedResourceNodes)
                return;

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
            var dirtyQueue = new OpenWorldChunkOverlayDirtyQueue();
            SW.SetResource(new OpenWorldPlacementIndexStore());
            SW.SetResource(new OpenWorldChunkOverlayStore(dirtyQueue));
            SW.SetResource(new OpenWorldPeerChunkOverlayState());
            SW.SetResource(dirtyQueue);

            if (OpenWorldResourcesCompatibility.UseLegacyReplicatedResourceNodes)
            {
                SW.SetResource(new OpenWorldResourceNodeFactory());
                SW.SetResource(new OpenWorldResourceNodeDeltaStore());
            }
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerOpenWorldResourcePlacementIndexSystem(), GameplaySystemOrder.ServerConnectionGameplay + 35);
            systems.Add(new ServerOpenWorldChunkOverlayRequestSystem(), GameplaySystemOrder.Gameplay - 50);
            systems.Add(new ServerOpenWorldChunkOverlaySendSystem(), GameplaySystemOrder.Gameplay - 45);

            if (OpenWorldResourcesCompatibility.UseLegacyReplicatedResourceNodes)
                systems.Add(new ServerOpenWorldResourceNodeDeltaCaptureSystem(), GameplaySystemOrder.CollectReplication - 30);
        }
    }
}
