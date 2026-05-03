using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Game.EcsViews
{
    public sealed class BindEntityViewSystem : ISystem
    {
        private readonly IEntityViewFactory _entityViewFactory;

        public BindEntityViewSystem(IEntityViewFactory entityViewFactory)
        {
            _entityViewFactory = entityViewFactory;
        }

        public void Update()
        {
            foreach (var entity in CW.Query<All<ViewPath>, None<View>>().Entities())
            {
                ref readonly var viewPath = ref entity.Read<ViewPath>();
                var view = _entityViewFactory.CreateViewForEntity(entity, in viewPath);
                entity.Set(new View(view));
            }
        }
    }
}
