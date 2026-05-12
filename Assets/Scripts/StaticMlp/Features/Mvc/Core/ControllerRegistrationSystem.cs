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
        private TController _controller;

        public ControllerRegistrationSystem(Func<IMvcManager, TController> controllerFactory)
        {
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public void Init()
        {
            var manager = CW.GetResource<MvcManagerResource>().Manager;
            _controller = _controllerFactory(manager);
            manager.RegisterController(_controller);
        }

        public void Update()
        {
        }
    }
}
