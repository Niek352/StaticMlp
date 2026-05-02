using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Bootstrap {
    public sealed class BuiltinGameplayFeature : GameplayFeature {
        public override void RegisterPrefabs() {
            PrefabRegistry.RegisterClient(Prefabs.Player, e => {
                e.Set<PlayerTag>();
                e.Set(new ViewTransform {
                    RenderRotation = UnityEngine.Quaternion.identity
                });
            });

            PrefabRegistry.RegisterServer(Prefabs.Player, e => e.Set<PlayerTag>());

            PrefabRegistry.RegisterClient(Prefabs.Monster, e => {
                e.Set<MonsterTag>();
                e.Set(new ViewTransform {
                    RenderRotation = UnityEngine.Quaternion.identity
                });
            });

            PrefabRegistry.RegisterServer(Prefabs.Monster, e => e.Set<MonsterTag>());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems) {
            systems.Add(new ServerPlayerJoinSpawnSystem(), GameplaySystemOrder.ServerConnectionGameplay);
            systems.Add(new ServerAiSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems) {
            systems.Add(new LocalPlayerMovementSystem(), GameplaySystemOrder.Gameplay);
            systems.Add(new LocalViewSyncSystem(), GameplaySystemOrder.ClientPresentation);
            systems.Add(new RemoteSmoothingSystem(), GameplaySystemOrder.ClientPresentation + 50);
        }
    }
}
