namespace Code.EcsUi.Mvc
{
    public delegate TView ViewFactoryMethod<out TView>()
        where TView : IView;
}
