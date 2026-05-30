using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public delegate void EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel>(
        TViewModel viewModel,
        in EcsWindowContext<TWorld, TWindow, TInput> context)
        where TWorld : struct, IWorldType
        where TWindow : struct, IEcsWindow
        where TViewModel : class, IViewModel;
}
