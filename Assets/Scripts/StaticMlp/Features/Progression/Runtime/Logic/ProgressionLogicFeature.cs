using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Progression
{
    public sealed class ProgressionLogicFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ReplicationRegistry.RegisterComponent<Stage1ProgressionState>(
                Stage1ProgressionStateReplication.TYPE_ID,
                Stage1ProgressionStateReplication.AUTHORITY,
                Stage1ProgressionStateReplication.AUDIENCE,
                Stage1ProgressionStateReplication.DELIVERY,
                Stage1ProgressionStateReplication.CreateDelta,
                Stage1ProgressionStateReplication.Read);
            ProjectionRegistry.Register<Stage1ProgressionState>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerStage1ProgressionAnchorInitSystem(), GameplaySystemOrder.Gameplay - 138);
            systems.Add(new ServerStage1RewardApplicationSystem(), GameplaySystemOrder.Gameplay - 93);
            systems.Add(new ServerStage1RaidDefenseProgressionSystem(), GameplaySystemOrder.Gameplay - 90);
            systems.Add(new ServerStage1BossPreparationProgressionSystem(), GameplaySystemOrder.Gameplay - 89);
        }
    }
}
