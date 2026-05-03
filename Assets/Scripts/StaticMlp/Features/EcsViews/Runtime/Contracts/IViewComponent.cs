using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.EcsViews
{
    public interface IViewComponent : IComponent, ITrackableAdded, ITrackableChanged
    {
    }
}
