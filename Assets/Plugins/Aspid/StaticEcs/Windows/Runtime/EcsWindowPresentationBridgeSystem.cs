using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public abstract class EcsWindowPresentationBridgeSystem<TWorld, TWindow, TSlot, TViewModel> : ISystem
        where TWorld : struct, IWorldType
        where TWindow : struct, IEcsWindow
        where TSlot : struct, IEcsWindowSlot
        where TViewModel : class, IViewModel
    {
        private WindowsController<TWorld> _windows;

        public void Init()
        {
            _windows = World<TWorld>.GetResource<WindowsController<TWorld>>();
        }

        public void Update()
        {
            if (!_windows.IsWindowActive<TWindow>())
                return;

            SyncPresentation(_windows.GetViewModel<TWindow, TSlot, TViewModel>());
        }

        protected abstract void SyncPresentation(TViewModel viewModel);
    }
}
