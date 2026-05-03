using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.EcsViews
{
    public readonly struct View : IComponent, ITrackableAdded
    {
        public readonly IEntityView Value;

        public View(IEntityView value)
        {
            Value = value;
        }
    }
}
