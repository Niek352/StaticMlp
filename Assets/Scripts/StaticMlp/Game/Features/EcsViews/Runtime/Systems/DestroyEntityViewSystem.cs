using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Game.EcsViews
{
    public sealed class DestroyEntityViewSystem : ISystem
    {
        private readonly IEntityViewFactory _entityViewFactory;

        public DestroyEntityViewSystem(IEntityViewFactory entityViewFactory)
        {
            _entityViewFactory = entityViewFactory;
        }

        public void Update()
        {
            foreach (var entity in CW.Query<All<View, DestroyViewRequest>>().Entities())
            {
                ref readonly var view = ref entity.Read<View>();
                _entityViewFactory.DestroyView(view.Value);
                entity.Delete<View>();
                entity.Delete<DestroyViewRequest>();
            }
        }
    }
}
