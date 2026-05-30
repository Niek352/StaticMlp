using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public sealed class StateDrivenEcsWindowHostSystem<TWorld, TWindow, TState> : ISystem
        where TWorld : struct, IWorldType
        where TWindow : struct, IEcsWindow
        where TState : struct, IResource
    {
        private readonly EcsWindowStatePredicate<TState> _isVisible;
        private WindowsController<TWorld> _windows;

        public StateDrivenEcsWindowHostSystem(EcsWindowStatePredicate<TState> isVisible)
        {
            _isVisible = isVisible ?? throw new System.ArgumentNullException(nameof(isVisible));
        }

        public void Init()
        {
            _windows = World<TWorld>.GetResource<WindowsController<TWorld>>();
        }

        public void Update()
        {
            ref readonly var state = ref World<TWorld>.GetResource<TState>();
            if (_isVisible(in state))
            {
                if (_windows.GetState<TWindow>() == EcsWindowState.ViewHidden)
                    _windows.Open<TWindow, EcsWindowNoData>(default);

                return;
            }

            _windows.TryClose<TWindow>();
        }
    }
}
