using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Bootstrap
{
    public sealed class BuiltinGameplayFeature : GameplayFeature
    {
        public const int PLAYER = 1;
        
        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(PLAYER, e =>
            {
                e.Set<PlayerTag>();
                e.Set(new ViewTransform
                {
                    RenderRotation = UnityEngine.Quaternion.identity
                });
            });

            NetArchetypeRegistry.RegisterServer(PLAYER, e => e.Set<PlayerTag>());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerPlayerJoinSpawnSystem(), GameplaySystemOrder.ServerConnectionGameplay);
            systems.Add(new ServerAiSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new LocalPlayerMovementSystem(), GameplaySystemOrder.Gameplay);
            ReplicationRegistry.RegisterClientCoreInterpolationSystems(systems);
            systems.Add(new LocalViewSyncSystem(), GameplaySystemOrder.ClientPresentation);
            systems.Add(new RemoteInterpolatedViewSyncSystem(), GameplaySystemOrder.ClientPresentation + 50);
        }
    }
}