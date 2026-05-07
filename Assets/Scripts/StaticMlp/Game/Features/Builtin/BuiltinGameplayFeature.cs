using StaticMlp.Game.Components;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Presentation;
using StaticMlp.Game.Systems;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Features.Builtin
{
    public sealed class BuiltinGameplayFeature : GameplayFeature
    {
        public const int PHYSICS_CUBE = 2;

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(PHYSICS_CUBE, e =>
            {
                e.Set<CubeTag>();
                e.Set(new ViewTransform
                {
                    RenderRotation = UnityEngine.Quaternion.identity
                });
            });

            NetArchetypeRegistry.RegisterServer(PHYSICS_CUBE, e => e.Set<CubeTag>());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerPeerDisconnectCleanupSystem(), GameplaySystemOrder.ServerConnectionGameplay - 1);
            systems.Add(new ServerSpawnCubeRequestSystem(), GameplaySystemOrder.Gameplay - 50);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new CubeSpawnInputSystem(), GameplaySystemOrder.Gameplay - 50);
            ReplicationRegistry.RegisterClientCoreInterpolationSystems(systems);
            systems.Add(new LocalViewSyncSystem(), GameplaySystemOrder.ClientPresentation);
            systems.Add(new RemoteInterpolatedViewSyncSystem(), GameplaySystemOrder.ClientPresentation + 50);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<ViewTransform>();
        }
    }
}
