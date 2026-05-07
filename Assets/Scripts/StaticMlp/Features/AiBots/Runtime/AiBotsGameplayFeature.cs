using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiBotsGameplayFeature : GameplayFeature
    {
        public const ushort BOT = 200;
        private const string CharacterViewPath = "Views/BotCharacterView";

        public override void RegisterNetworkEvents()
        {
            AiBotNetworkEvents.Register();
        }

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(BOT, e =>
            {
                e.Set<MonsterTag>();
                e.Set<AiAgentTag>();
                e.Set(new ViewPath(CharacterViewPath));
                e.Set(new ViewTransform
                {
                    RenderRotation = Quaternion.identity
                });
            });

            NetArchetypeRegistry.RegisterServer(BOT, e =>
            {
                e.Set<MonsterTag>();
                e.Set<AiAgentTag>();
            });
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerAiRuntimeInitSystem(), GameplaySystemOrder.ServerConnectionGameplay + 20);
            systems.Add(new ServerAiBotSeedSystem(), GameplaySystemOrder.ServerConnectionGameplay + 30);
            systems.Add(new ServerCommandBotRequestSystem(), GameplaySystemOrder.Gameplay - 120);
            systems.Add(new ServerAiNeedsSystem(), GameplaySystemOrder.Gameplay - 80);
            systems.Add(new ServerAiPerceptionSystem(), GameplaySystemOrder.Gameplay - 70);
            systems.Add(new ServerAiWorkTargetSystem(), GameplaySystemOrder.Gameplay - 60);
            systems.Add(new ServerAiUtilityDecisionSystem(), GameplaySystemOrder.Gameplay - 50);
            systems.Add(new ServerAiTaskExecutionSystem(), GameplaySystemOrder.Gameplay - 40);
            systems.Add(new ServerAiMovementSystem(), GameplaySystemOrder.Gameplay - 30);
            systems.Add(new ServerAiNetStateSystem(), GameplaySystemOrder.CollectReplication - 10);
        }
    }
}
