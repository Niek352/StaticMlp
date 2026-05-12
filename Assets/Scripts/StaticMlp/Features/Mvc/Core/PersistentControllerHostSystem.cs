using System;
using FFS.Libraries.StaticEcs;
using Cysharp.Threading.Tasks;
using StaticMlp.Networking;

namespace Code.EcsUi.Mvc
{
    public sealed class PersistentControllerHostSystem<TView, TController> : ISystem
        where TView : IView
        where TController : class, IController<TView, ControllerNoData>
    {
        private readonly Func<IMvcManager, TController> _controllerFactory;

        private IMvcManager _mvcManager;
        private TController _controller;

        public PersistentControllerHostSystem(Func<IMvcManager, TController> controllerFactory)
        {
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public void Init()
        {
            _mvcManager = CW.GetResource<MvcManagerResource>().Manager;
            _controller = _controllerFactory(_mvcManager);
            _mvcManager.RegisterController(_controller);
        }

        public void Update()
        {
            if (_controller.State == ControllerState.ViewHidden)
            {
                _mvcManager.ShowAsync(ControllerBase<TView>.IssueCommand()).Forget();
                return;
            }
        }
    }
}
