using StaticMlp.Networking;

namespace StaticMlp.Game.EcsViews
{
    public interface IEntityViewFactory
    {
        IEntityView CreateViewForEntity(CW.Entity entity, in ViewPath viewPath);
        void DestroyView(IEntityView view);
    }
}
