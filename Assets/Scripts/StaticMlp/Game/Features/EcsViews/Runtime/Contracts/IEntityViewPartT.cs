namespace StaticMlp.Game.EcsViews
{
    public interface IEntityViewPart<TComponent> : IEntityViewPart
        where TComponent : struct, IViewComponent
    {
        void Apply(in TComponent component);
    }
}
