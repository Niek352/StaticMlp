using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public sealed class PersistentEcsWindowHostSystem<TWorld, TWindow> : ISystem
        where TWorld : struct, IWorldType
        where TWindow : struct, IEcsWindow
    {
        private WindowsController<TWorld> _windows;

        public void Init()
        {
            _windows = World<TWorld>.GetResource<WindowsController<TWorld>>();
        }

        public void Update()
        {
            if (_windows.GetState<TWindow>() == EcsWindowState.ViewHidden)
                _windows.Open<TWindow, EcsWindowNoData>(default);
        }
    }
}
