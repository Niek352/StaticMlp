using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Progression
{
    public sealed class ProgressionLogicFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<Stage1ProgressionState>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerStage1RewardApplicationSystem(), GameplaySystemOrder.Gameplay - 93);
            systems.Add(new ServerStage1RaidDefenseProgressionSystem(), GameplaySystemOrder.Gameplay - 90);
            systems.Add(new ServerStage1BossPreparationProgressionSystem(), GameplaySystemOrder.Gameplay - 89);
        }
    }
}
