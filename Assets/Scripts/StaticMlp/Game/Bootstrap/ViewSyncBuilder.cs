using FFS.Libraries.StaticEcs;
using StaticMlp.Game.EcsViews;

namespace StaticMlp.Game.Bootstrap
{
    public readonly struct ViewSyncBuilder
    {
        private readonly ClientCoreSystemsBuilder _systems;
        private readonly short _order;

        public ViewSyncBuilder(ClientCoreSystemsBuilder systems, short order)
        {
            _systems = systems;
            _order = order;
        }

        public void Register<TComponent>()
            where TComponent : struct, IViewComponent
        {
            _systems.Add(new ApplyComponentToViewSystem<TComponent>(), _order);
        }

        public void Add<TSystem>(TSystem system, short order)
            where TSystem : ISystem
        {
            _systems.Add(system, order);
        }
    }
}
