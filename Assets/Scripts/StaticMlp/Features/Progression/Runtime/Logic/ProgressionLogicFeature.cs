using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Progression
{
    public sealed class ProgressionLogicFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerStage1ProgressionAnchorInitSystem(), GameplaySystemOrder.Gameplay - 138);
            systems.Add(new ServerStage1RewardApplicationSystem(), GameplaySystemOrder.Gameplay - 93);
            systems.Add(new ServerStage1RaidDefenseProgressionSystem(), GameplaySystemOrder.Gameplay - 90);
        }
    }
}
