using FFS.Libraries.StaticEcs;

namespace StaticMlp.Game.EcsViews
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
