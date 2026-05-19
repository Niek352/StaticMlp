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
    }
}
