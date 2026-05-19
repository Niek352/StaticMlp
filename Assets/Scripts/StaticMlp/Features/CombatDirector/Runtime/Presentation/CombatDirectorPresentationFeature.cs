using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class CombatDirectorPresentationFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new DirectorTelegraphReceiveSystem(), (short)(ViewSystemOrder.BuildPresentationState - 20));
            systems.Add(new SpawnSourceVfxSystem(), (short)(ViewSystemOrder.BuildPresentationState - 19));
            systems.Add(new EnemySpawnAudioSystem(), (short)(ViewSystemOrder.BuildPresentationState - 18));
            systems.Add(new EnemyViewBindSystem(), (short)(ViewSystemOrder.BuildPresentationState - 17));
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<EnemyRoleViewState>();
        }
    }
}
