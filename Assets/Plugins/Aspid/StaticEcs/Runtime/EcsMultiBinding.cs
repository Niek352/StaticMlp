using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs
{
    public delegate void EcsMultiBinding<TWorld, in TViewModel, TElement>(
        TViewModel viewModel,
        in World<TWorld>.Multi<TElement> multi)
        where TWorld : struct, IWorldType
        where TViewModel : class, IViewModel
        where TElement : struct, IMultiComponent;
}
