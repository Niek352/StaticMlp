using System;
using FFS.Libraries.StaticEcs;
using Cysharp.Threading.Tasks;
using StaticMlp.Networking;

namespace Code.EcsUi.Mvc
{
    public sealed class PopupControllerHostSystem<TView, TController> : ISystem
        where TView : IView
        where TController : class, IController<TView, ControllerNoData>
    {
        private readonly Func<IMvcManager, TController> _controllerFactory;
        private readonly Func<bool> _shouldBeVisible;

        private IMvcManager _mvcManager;
        private TController _controller;

        public PopupControllerHostSystem(
            Func<IMvcManager, TController> controllerFactory,
            Func<bool> shouldBeVisible)
        {
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            _shouldBeVisible = shouldBeVisible ?? throw new ArgumentNullException(nameof(shouldBeVisible));
        }

        public void Init()
        {
            _mvcManager = CW.GetResource<MvcManagerResource>().Manager;
            _controller = _controllerFactory(_mvcManager);
            _mvcManager.RegisterController(_controller);
        }

        public void Update()
        {
            var shouldBeVisible = _shouldBeVisible();

            if (shouldBeVisible)
            {
                if (_controller.State == ControllerState.ViewHidden)
                    _mvcManager.ShowAsync(ControllerBase<TView>.IssueCommand()).Forget();

                return;
            }

            if (_controller.State != ControllerState.ViewHidden)
                _mvcManager.TryClose(_controller);
        }
    }
}
