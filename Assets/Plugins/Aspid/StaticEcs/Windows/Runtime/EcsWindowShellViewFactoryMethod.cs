namespace Aspid.StaticEcs.Windows
{
    public delegate TView EcsWindowShellViewFactoryMethod<out TView>()
        where TView : IEcsWindowShellView;
}
