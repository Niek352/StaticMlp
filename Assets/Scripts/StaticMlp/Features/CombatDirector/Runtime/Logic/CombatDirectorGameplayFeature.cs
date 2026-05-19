using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class CombatDirectorGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<DirectorState>();
            ProjectionRegistry.Register<EnemyArchetype>();
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(EncounterDirectorConfig.CreateDefault());
            SW.SetResource(EnemySpawnCatalog.CreateDefault());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new CombatCellTrackingSystem(), GameplaySystemOrder.Gameplay - 99);
            systems.Add(new PlayerThreatInputSystem(), GameplaySystemOrder.Gameplay - 20);
            systems.Add(new ThreatBudgetAccumulationSystem(), GameplaySystemOrder.Gameplay - 19);
            systems.Add(new DirectorPhaseSystem(), GameplaySystemOrder.Gameplay - 18);
        }
    }
}
