using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace Code.EcsUi.Mvc
{
    public sealed class ControllerRegistrationSystem<TView, TController> : ISystem
        where TView : IView
        where TController : class, IController<TView, ControllerNoData>
    {
        private readonly Func<IMvcManager, TController> _controllerFactory;
        private readonly Action<TController> _syncActiveController;

        private TController _controller;

        public ControllerRegistrationSystem(
            Func<IMvcManager, TController> controllerFactory,
            Action<TController> syncActiveController = null)
        {
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            _syncActiveController = syncActiveController;
        }

        public void Init()
        {
            var manager = CW.GetResource<MvcManagerResource>().Manager;
            _controller = _controllerFactory(manager);
            manager.RegisterController(_controller);
        }

        public void Update()
        {
            if (_controller.State == ControllerState.ViewHidden)
                return;

            _syncActiveController?.Invoke(_controller);
        }
    }
}
