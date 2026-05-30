using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs
{
    public delegate void EcsTagBinding<in TViewModel, TTag>(TViewModel viewModel, bool isPresent)
        where TViewModel : class, IViewModel
        where TTag : struct, ITag;
}
