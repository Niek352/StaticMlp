using System;
using FFS.Libraries.StaticEcs;
using Cysharp.Threading.Tasks;
using StaticMlp.Networking;

namespace Code.EcsUi.Mvc
{
    public sealed class StateDrivenPersistentControllerHostSystem<TView, TController, TState> : ISystem
        where TView : IView
        where TController : class, IController<TView, ControllerNoData>
        where TState : struct, IResource
    {
        private readonly Func<IMvcManager, TController> _controllerFactory;
        private readonly Func<TState, bool> _shouldBeVisible;
        private readonly Action<TController, TState> _syncActiveController;

        private IMvcManager _mvcManager;
        private TController _controller;

        public StateDrivenPersistentControllerHostSystem(
            Func<IMvcManager, TController> controllerFactory,
            Func<TState, bool> shouldBeVisible,
            Action<TController, TState> syncActiveController = null)
        {
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            _shouldBeVisible = shouldBeVisible ?? throw new ArgumentNullException(nameof(shouldBeVisible));
            _syncActiveController = syncActiveController;
        }

        public void Init()
        {
            _mvcManager = CW.GetResource<MvcManagerResource>().Manager;
            _controller = _controllerFactory(_mvcManager);
            _mvcManager.RegisterController(_controller);
        }

        public void Update()
        {
            var state = CW.GetResource<TState>();
            var shouldBeVisible = _shouldBeVisible(state);

            if (shouldBeVisible)
            {
                if (_controller.State == ControllerState.ViewHidden)
                    _mvcManager.ShowAsync(ControllerBase<TView>.IssueCommand()).Forget();
                else
                    _syncActiveController?.Invoke(_controller, state);

                return;
            }

            if (_controller.State != ControllerState.ViewHidden)
                _mvcManager.TryClose(_controller);
        }
    }
}
