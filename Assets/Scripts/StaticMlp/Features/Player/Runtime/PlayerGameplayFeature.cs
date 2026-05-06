using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Player
{
    public sealed class PlayerGameplayFeature : GameplayFeature
    {
        public const int PLAYER = 1;
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
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerPlayerJoinSpawnSystem(), GameplaySystemOrder.ServerConnectionGameplay);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientCameraInputSystem(), (short)(GameplaySystemOrder.ClientInput + 20));
            systems.Add(new LocalPlayerMovementSystem(), GameplaySystemOrder.Gameplay);
        }
    }
}
