using StaticMlp.Networking;

namespace StaticMlp.Features.EcsViews
{
    public interface IEntityView
    {
        CW.Entity Entity { get; }

        void Bind(CW.Entity entity);
        void Unbind();

        void Apply<TComponent>(in TComponent component)
            where TComponent : struct, IViewComponent;
    }
}
