namespace StaticMlp.Game.EcsViews
{
    public interface IEntityViewPart
    {
        void OnBind(IEntityView view);
        void OnUnbind();
    }
}
