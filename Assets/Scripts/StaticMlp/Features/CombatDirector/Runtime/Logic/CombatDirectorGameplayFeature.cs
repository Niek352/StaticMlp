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
            ProjectionRegistry.Register<EnemySpawnSource>();
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(EncounterDirectorConfig.CreateDefault());
            SW.SetResource(EnemySpawnCatalog.CreateDefault());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new SpawnSourcePlacementSeedSystem(), GameplaySystemOrder.Gameplay - 100);
            systems.Add(new CombatCellTrackingSystem(), GameplaySystemOrder.Gameplay - 99);
            systems.Add(new PlayerThreatInputSystem(), GameplaySystemOrder.Gameplay - 20);
            systems.Add(new CellAttentionInputSystem(), GameplaySystemOrder.Gameplay - 19);
            systems.Add(new CellAttentionDecaySystem(), GameplaySystemOrder.Gameplay - 18);
            systems.Add(new DirectorPhaseSystem(), GameplaySystemOrder.Gameplay - 17);
            systems.Add(new SpawnSourceSelectionSystem(), GameplaySystemOrder.Gameplay - 16);
            systems.Add(new SpawnRequestBuildSystem(), GameplaySystemOrder.Gameplay - 15);
            systems.Add(new SpawnRequestValidationSystem(), GameplaySystemOrder.Gameplay - 14);
            systems.Add(new EnemySpawnApplySystem(), GameplaySystemOrder.Gameplay - 13);
        }
    }
}
