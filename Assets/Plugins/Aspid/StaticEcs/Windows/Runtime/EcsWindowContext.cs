using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public readonly struct EcsWindowContext<TWorld, TWindow, TInput>
        where TWorld : struct, IWorldType
        where TWindow : struct, IEcsWindow
    {
        public readonly WindowsController<TWorld> Windows;
        public readonly TInput Input;

        public EcsWindowContext(WindowsController<TWorld> windows, TInput input)
        {
            Windows = windows;
            Input = input;
        }
    }
}
