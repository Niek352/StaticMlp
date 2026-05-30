using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public interface IEcsWindowCloseRequest<TWindow> : IEvent
        where TWindow : struct, IEcsWindow
    {
    }
}
