using FFS.Libraries.StaticEcs;

namespace StaticMlp.Game.EcsViews
{
    public interface IViewComponent : IComponent, ITrackableAdded, ITrackableChanged
    {
    }
}
