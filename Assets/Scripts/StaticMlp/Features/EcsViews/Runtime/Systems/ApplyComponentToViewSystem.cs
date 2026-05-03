using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.EcsViews
{
    public sealed class ApplyComponentToViewSystem<TComponent> : ISystem
        where TComponent : struct, IViewComponent
    {
        public void Update()
        {
            foreach (var entity in CW.Query<All<View, TComponent>, AllAdded<View>>().Entities())
                Apply(entity);

            foreach (var entity in CW.Query<All<View, TComponent>, AllAdded<TComponent>>().Entities())
                Apply(entity);

            foreach (var entity in CW.Query<All<View, TComponent>, AllChanged<TComponent>, NoneAdded<TComponent>>().Entities())
                Apply(entity);
        }

        private static void Apply(CW.Entity entity)
        {
            ref readonly var view = ref entity.Read<View>();
            ref readonly var component = ref entity.Read<TComponent>();
            view.Value.Apply(in component);
        }
    }
}
