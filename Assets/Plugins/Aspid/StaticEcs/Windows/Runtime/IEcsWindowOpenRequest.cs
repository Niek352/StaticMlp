using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public interface IEcsWindowOpenRequest<TWindow, out TInput> : IEvent
        where TWindow : struct, IEcsWindow
    {
        TInput Input { get; }
    }
}
