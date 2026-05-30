using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public sealed class EcsWindowRequestSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private WindowsController<TWorld> _controller;
        private EventReceiver<TWorld, CloseTopEcsWindowRequest> _closeTopRequests;
        private EventReceiver<TWorld, CloseAllEcsWindowsRequest> _closeAllRequests;
        private EventReceiver<TWorld, SetAllEcsWindowsPresentationActiveRequest> _presentationActiveRequests;

        public void Init()
        {
            _controller = World<TWorld>.GetResource<WindowsController<TWorld>>();
            _closeTopRequests = World<TWorld>.RegisterEventReceiver<CloseTopEcsWindowRequest>();
            _closeAllRequests = World<TWorld>.RegisterEventReceiver<CloseAllEcsWindowsRequest>();
            _presentationActiveRequests = World<TWorld>.RegisterEventReceiver<SetAllEcsWindowsPresentationActiveRequest>();
            _controller.InitializeRequestReceivers();
        }

        public void Update()
        {
            foreach (var _ in _closeTopRequests)
                _controller.TryCloseTopClosable();

            foreach (var _ in _closeAllRequests)
                _controller.TryCloseAll();

            foreach (var evt in _presentationActiveRequests)
                _controller.SetAllWindowsPresentationActive(evt.Value.IsActive);

            _controller.ProcessRequestReceivers();
        }

        public void Destroy()
        {
            _controller.DestroyRequestReceivers();
            World<TWorld>.DeleteEventReceiver(ref _closeTopRequests);
            World<TWorld>.DeleteEventReceiver(ref _closeAllRequests);
            World<TWorld>.DeleteEventReceiver(ref _presentationActiveRequests);
        }
    }
}
