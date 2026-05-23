using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Progression
{
    public sealed class ProgressionLogicFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<ProgressionState>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerRewardApplicationSystem(), GameplaySystemOrder.Gameplay - 93);
            systems.Add(new ServerRaidDefenseProgressionSystem(), GameplaySystemOrder.Gameplay - 90);
            systems.Add(new ServerBossPreparationProgressionSystem(), GameplaySystemOrder.Gameplay - 89);
        }
    }
}
