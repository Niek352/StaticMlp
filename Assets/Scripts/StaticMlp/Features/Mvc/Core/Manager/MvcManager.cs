using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public sealed class MvcManager : IMvcManager
    {
        private readonly Dictionary<Type, IController> controllers = new();
        private readonly IWindowStackManager windowStackManager;
        private readonly CancellationTokenSource destructionCts;

        public MvcManager(IWindowStackManager windowStackManager)
        {
            this.windowStackManager = windowStackManager ?? throw new ArgumentNullException(nameof(windowStackManager));
            destructionCts = new CancellationTokenSource();
        }

        public event Action<IController> OnViewShown;
        public event Action<IController> OnViewClosed;

        public void RegisterController<TView, TInputData>(IController<TView, TInputData> controller)
            where TView : IView
        {
            controllers.Add(typeof(IController<TView, TInputData>), controller);
        }

        public void SetAllViewsPresentationActive(bool isActive)
        {
            foreach (var controller in controllers.Values)
                controller.SetViewPresentationActive(isActive);
        }

        public void SetAllViewsPresentationActive(IController except, bool isActive)
        {
            foreach (var controller in controllers.Values)
                controller.SetViewPresentationActive(controller != except && isActive);
        }

        public bool TryCloseTopClosable()
        {
            return windowStackManager.TryCloseTopClosable();
        }

        public bool TryClose(IController controller)
        {
            return windowStackManager.TryClose(controller);
        }

        public async UniTask ShowAsync<TView, TInputData>(ShowCommand<TView, TInputData> command, CancellationToken ct = default)
            where TView : IView
        {
            var controller = controllers[typeof(IController<TView, TInputData>)];

            if (controller.State != ControllerState.ViewHidden)
                return;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                ct == default ? CancellationToken.None : ct,
                destructionCts.Token);

            var token = linkedCts.Token;
            OnViewShown?.Invoke(controller);

            try
            {
                switch (controller.Layer)
                {
                    case ViewLayer.Popup:
                        await ShowPopupAsync(command, controller, token);
                        break;
                    case ViewLayer.Fullscreen:
                        await ShowFullscreenAsync(command, controller, token);
                        break;
                    case ViewLayer.Persistent:
                        await ShowPersistentAsync(command, controller, token);
                        break;
                    case ViewLayer.Overlay:
                        await ShowOverlayAsync(command, controller, token);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                OnViewClosed?.Invoke(controller);
            }
        }

        public void Dispose()
        {
            destructionCts.Cancel();

            foreach (var controller in controllers.Values)
                controller.Dispose();

            destructionCts.Dispose();
            windowStackManager.Dispose();
        }

        private async UniTask ShowPopupAsync<TView, TInputData>(
            ShowCommand<TView, TInputData> command,
            IController controller,
            CancellationToken ct)
            where TView : IView
        {
            var pushInfo = windowStackManager.PushPopup(controller);
            var closure = windowStackManager.GetControllerClosure(controller);

            try
            {
                pushInfo.PreviousController?.Blur();

                await UniTask.WhenAny(
                    command.Execute(controller, pushInfo.ControllerOrdering, ct),
                    pushInfo.OnClose?.Task ?? UniTask.Never(ct),
                    closure?.Task ?? UniTask.Never(ct));

                await controller.HideViewAsync(ct);
            }
            finally
            {
                var popInfo = windowStackManager.PopPopup(controller, false);
                popInfo.NewTopMostController?.Focus();
            }
        }

        private async UniTask ShowFullscreenAsync<TView, TInputData>(
            ShowCommand<TView, TInputData> command,
            IController controller,
            CancellationToken ct)
            where TView : IView
        {
            if (windowStackManager.CurrentFullscreenController == controller)
                return;

            if (windowStackManager.CurrentFullscreenController != null)
                windowStackManager.PopFullscreen(windowStackManager.CurrentFullscreenController);

            var pushInfo = windowStackManager.PushFullscreen(controller);
            var closure = windowStackManager.GetControllerClosure(controller);

            try
            {
                CloseControllers(pushInfo.PopupControllers.Select(x => x.controller));

                await UniTask.WhenAny(
                    command.Execute(controller, pushInfo.ControllerOrdering, ct),
                    pushInfo.OnClose?.Task ?? UniTask.Never(ct),
                    closure?.Task ?? UniTask.Never(ct));

                await controller.HideViewAsync(ct);
            }
            finally
            {
                windowStackManager.PopFullscreen(controller);
            }
        }

        private async UniTask ShowPersistentAsync<TView, TInputData>(
            ShowCommand<TView, TInputData> command,
            IController controller,
            CancellationToken ct)
            where TView : IView
        {
            var pushInfo = windowStackManager.PushPersistent(controller);
            var closure = windowStackManager.GetControllerClosure(controller);

            try
            {
                await UniTask.WhenAny(
                    command.Execute(controller, pushInfo.ControllerOrdering, ct),
                    closure?.Task ?? UniTask.Never(ct));

                await controller.HideViewAsync(ct);
            }
            finally
            {
                windowStackManager.RemovePersistent(controller);
            }
        }

        private async UniTask ShowOverlayAsync<TView, TInputData>(
            ShowCommand<TView, TInputData> command,
            IController controller,
            CancellationToken ct)
            where TView : IView
        {
            var pushInfo = windowStackManager.PushOverlay(controller);
            var closure = windowStackManager.GetControllerClosure(controller);

            try
            {
                CloseControllers(pushInfo.PopupControllers.Select(x => x.controller));

                if (pushInfo.FullscreenController != null)
                    windowStackManager.PopFullscreen(pushInfo.FullscreenController);

                await UniTask.WhenAny(
                    command.Execute(controller, pushInfo.ControllerOrdering, ct),
                    closure?.Task ?? UniTask.Never(ct));

                await controller.HideViewAsync(ct);
            }
            finally
            {
                windowStackManager.PopOverlay(controller);
            }
        }

        private void CloseControllers(IEnumerable<IController> controllersToClose)
        {
            foreach (var controller in controllersToClose.ToArray())
                windowStackManager.TryClose(controller);
        }
    }
}
