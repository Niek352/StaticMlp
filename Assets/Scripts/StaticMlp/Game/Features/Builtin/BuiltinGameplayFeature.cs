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
        public const int PLAYER = 1;
        public const int PHYSICS_CUBE = 2;
        private const string PlayerViewPath = "Views/CharacterView";

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(PLAYER, e =>
            {
                e.Set<PlayerTag>();
                e.Set(new ViewPath(PlayerViewPath));
                e.Set(new ViewTransform
                {
                    RenderRotation = UnityEngine.Quaternion.identity
                });
            });

            NetArchetypeRegistry.RegisterServer(PLAYER, e => e.Set<PlayerTag>());

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
            systems.Add(new ServerPlayerJoinSpawnSystem(), GameplaySystemOrder.ServerConnectionGameplay);
            systems.Add(new ServerSpawnCubeRequestSystem(), GameplaySystemOrder.Gameplay - 50);
            systems.Add(new ServerAiSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new CubeSpawnInputSystem(), GameplaySystemOrder.Gameplay - 50);
            systems.Add(new LocalPlayerMovementSystem(), GameplaySystemOrder.Gameplay);
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
