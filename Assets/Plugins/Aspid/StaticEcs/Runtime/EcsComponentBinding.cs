using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs
{
    public delegate void EcsComponentBinding<in TViewModel, TComponent>(
        TViewModel viewModel,
        in TComponent component)
        where TViewModel : class, IViewModel
        where TComponent : struct, IComponent;
}
