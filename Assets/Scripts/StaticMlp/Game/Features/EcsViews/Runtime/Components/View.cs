using FFS.Libraries.StaticEcs;

namespace StaticMlp.Game.EcsViews
{
    public readonly struct View : IComponent
    {
        public readonly IEntityView Value;

        public View(IEntityView value)
        {
            Value = value;
        }
    }
}
