namespace StaticMlp.Features.EcsViews
{
    public interface IEntityViewPart
    {
        void OnBind(IEntityView view);
        void OnUnbind();
    }
}
