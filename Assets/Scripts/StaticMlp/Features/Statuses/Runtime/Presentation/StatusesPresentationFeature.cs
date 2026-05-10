using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Statuses
{
    public sealed class StatusesPresentationFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientStatusAuraViewStateSystem(), (short)(ViewSystemOrder.BuildPresentationState + 6));
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<StatusAuraViewState>();
        }
    }
}
