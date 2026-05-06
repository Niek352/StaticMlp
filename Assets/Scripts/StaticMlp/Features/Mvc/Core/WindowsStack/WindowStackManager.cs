using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public sealed class WindowStackManager : IWindowStackManager
    {
        private const int PopupOrderIncrement = 2;
        private const int MinimumPopupBackdropOrder = -3;

        private readonly List<(IController controller, int orderInLayer)> popupStack = new();
        private readonly List<(IController controller, int sortOrder)> persistentStack = new();
        private readonly List<(IController controller, UniTaskCompletionSource closer)> closeableStack = new();
        private readonly List<(IController controller, UniTaskCompletionSource closer)> controllerClosures = new();

        private IController fullscreenController;
        private IController overlayController;

        public (IController controller, int orderInLayer) TopMostPopup => popupStack.LastOrDefault();

        public IController CurrentFullscreenController => fullscreenController;

        public void Dispose()
        {
            popupStack.Clear();
            persistentStack.Clear();
            closeableStack.Clear();
            controllerClosures.Clear();
        }

        public PopupPushInfo PushPopup(IController controller)
        {
            var currentMaxOrder = PopupOrderIncrement;
            var topMostPopup = TopMostPopup;

            if (topMostPopup.controller != null)
                currentMaxOrder = topMostPopup.orderInLayer + PopupOrderIncrement;

            popupStack.Add((controller, currentMaxOrder));
            var onClose = AddCloseableIfNeeded(controller);
            AddControllerClosure(controller);

            foreach (var persistent in persistentStack)
                if (persistent.controller.State == ControllerState.ViewFocused)
                    persistent.controller.Blur();

            return new PopupPushInfo(
                new ViewOrdering(ViewLayer.Popup, currentMaxOrder),
                new ViewOrdering(ViewLayer.Popup, currentMaxOrder - 1),
                popupStack.Count >= 2 ? popupStack[^2].controller : null,
                onClose);
        }

        public FullscreenPushInfo PushFullscreen(IController controller)
        {
            fullscreenController = controller;
            var onClose = AddCloseableIfNeeded(controller);
            AddControllerClosure(controller);

            foreach (var persistent in persistentStack)
                if (persistent.controller.State == ControllerState.ViewFocused)
                    persistent.controller.Blur();

            return new FullscreenPushInfo(popupStack.ToList(), new ViewOrdering(ViewLayer.Fullscreen, 0), onClose);
        }

        public void PopFullscreen(IController controller)
        {
            TryCompleteControllerClosure(controller);

            foreach (var persistent in persistentStack)
                persistent.controller.Focus();

            if (fullscreenController == controller)
                fullscreenController = null;

            TryRemoveCloseable(controller);
        }

        public PersistentPushInfo PushPersistent(IController controller)
        {
            var sortOrder = GetRequiredPersistentSortOrder(controller);
            EnsureUniquePersistentSortOrder(controller, sortOrder);

            persistentStack.Add((controller, sortOrder));
            AddControllerClosure(controller);
            return new PersistentPushInfo(new ViewOrdering(ViewLayer.Persistent, sortOrder));
        }

        public void RemovePersistent(IController controller)
        {
            TryCompleteControllerClosure(controller);
            RemovePersistentInternal(controller);
        }

        public OverlayPushInfo PushOverlay(IController controller)
        {
            overlayController = controller;
            AddControllerClosure(controller);
            return new OverlayPushInfo(popupStack.ToList(), fullscreenController, new ViewOrdering(ViewLayer.Overlay, 1));
        }

        public void PopOverlay(IController controller)
        {
            TryCompleteControllerClosure(controller);

            if (overlayController == controller)
                overlayController = null;
        }

        public PopupPopInfo PopPopup(IController controller, bool shouldGracefullyClose = true)
        {
            RemovePopup(controller);

            if (shouldGracefullyClose)
                TryCompleteControllerClosure(controller);

            if (popupStack.Count == 0)
            {
                foreach (var persistent in persistentStack)
                    if (persistent.controller.State == ControllerState.ViewBlurred)
                        persistent.controller.Focus();
            }

            TryRemoveCloseable(controller);

            var topMostPopup = TopMostPopup;
            var ordering = new ViewOrdering(
                ViewLayer.Popup,
                topMostPopup.controller == null ? MinimumPopupBackdropOrder : topMostPopup.orderInLayer - 1);

            return new PopupPopInfo(ordering, topMostPopup.controller);
        }

        public UniTaskCompletionSource GetControllerClosure(IController controller)
        {
            for (var i = 0; i < controllerClosures.Count; i++)
                if (controllerClosures[i].controller == controller)
                    return controllerClosures[i].closer;

            return null;
        }

        public bool TryCloseTopClosable()
        {
            if (closeableStack.Count == 0)
                return false;

            closeableStack[^1].closer.TrySetResult();
            return true;
        }

        public bool TryClose(IController controller)
        {
            for (var i = 0; i < closeableStack.Count; i++)
            {
                if (closeableStack[i].controller != controller)
                    continue;

                closeableStack[i].closer.TrySetResult();
                return true;
            }

            return TryCompleteControllerClosure(controller);
        }

        private UniTaskCompletionSource AddCloseableIfNeeded(IController controller)
        {
            if (!controller.CanBeClosedByEscape)
                return null;

            var onClose = new UniTaskCompletionSource();
            closeableStack.Add((controller, onClose));
            return onClose;
        }

        private void AddControllerClosure(IController controller)
        {
            controllerClosures.Add((controller, new UniTaskCompletionSource()));
        }

        private bool TryCompleteControllerClosure(IController controller)
        {
            for (var i = 0; i < controllerClosures.Count; i++)
            {
                if (controllerClosures[i].controller != controller)
                    continue;

                var closer = controllerClosures[i].closer;
                controllerClosures.RemoveAt(i);
                closer.TrySetResult();
                return true;
            }

            return false;
        }

        private void RemovePopup(IController controller)
        {
            for (var i = 0; i < popupStack.Count; i++)
            {
                if (popupStack[i].controller != controller)
                    continue;

                popupStack.RemoveAt(i);
                break;
            }
        }

        private void TryRemoveCloseable(IController controller)
        {
            for (var i = 0; i < closeableStack.Count; i++)
            {
                if (closeableStack[i].controller != controller)
                    continue;

                closeableStack.RemoveAt(i);
                break;
            }
        }

        private static int GetRequiredPersistentSortOrder(IController controller)
        {
            if (controller.PersistentSortOrder.HasValue)
                return controller.PersistentSortOrder.Value;

            throw new System.InvalidOperationException(
                $"{controller.GetType().Name} uses {ViewLayer.Persistent} but does not declare {nameof(IController.PersistentSortOrder)}.");
        }

        private void EnsureUniquePersistentSortOrder(IController controller, int sortOrder)
        {
            for (var i = 0; i < persistentStack.Count; i++)
            {
                var existing = persistentStack[i];
                if (existing.sortOrder != sortOrder)
                    continue;

                throw new System.InvalidOperationException(
                    $"{controller.GetType().Name} and {existing.controller.GetType().Name} share the same persistent sort order {sortOrder}.");
            }
        }

        private void RemovePersistentInternal(IController controller)
        {
            for (var i = 0; i < persistentStack.Count; i++)
            {
                if (persistentStack[i].controller != controller)
                    continue;

                persistentStack.RemoveAt(i);
                break;
            }
        }
    }

    public readonly struct PopupPushInfo
    {
        public readonly ViewOrdering ControllerOrdering;
        public readonly ViewOrdering PopupBackdropOrdering;
        public readonly IController PreviousController;
        public readonly UniTaskCompletionSource OnClose;

        public PopupPushInfo(
            ViewOrdering controllerOrdering,
            ViewOrdering popupBackdropOrdering,
            IController previousController,
            UniTaskCompletionSource onClose)
        {
            ControllerOrdering = controllerOrdering;
            PopupBackdropOrdering = popupBackdropOrdering;
            PreviousController = previousController;
            OnClose = onClose;
        }
    }

    public readonly struct PopupPopInfo
    {
        public readonly ViewOrdering PopupBackdropOrdering;
        public readonly IController NewTopMostController;

        public PopupPopInfo(ViewOrdering popupBackdropOrdering, IController newTopMostController)
        {
            PopupBackdropOrdering = popupBackdropOrdering;
            NewTopMostController = newTopMostController;
        }
    }

    public readonly struct FullscreenPushInfo
    {
        public readonly List<(IController controller, int orderInLayer)> PopupControllers;
        public readonly ViewOrdering ControllerOrdering;
        public readonly UniTaskCompletionSource OnClose;

        public FullscreenPushInfo(
            List<(IController controller, int orderInLayer)> popupControllers,
            ViewOrdering controllerOrdering,
            UniTaskCompletionSource onClose)
        {
            PopupControllers = popupControllers;
            ControllerOrdering = controllerOrdering;
            OnClose = onClose;
        }
    }

    public readonly struct OverlayPushInfo
    {
        public readonly List<(IController controller, int orderInLayer)> PopupControllers;
        public readonly IController FullscreenController;
        public readonly ViewOrdering ControllerOrdering;

        public OverlayPushInfo(
            List<(IController controller, int orderInLayer)> popupControllers,
            IController fullscreenController,
            ViewOrdering controllerOrdering)
        {
            PopupControllers = popupControllers;
            FullscreenController = fullscreenController;
            ControllerOrdering = controllerOrdering;
        }
    }

    public readonly struct PersistentPushInfo
    {
        public readonly ViewOrdering ControllerOrdering;

        public PersistentPushInfo(ViewOrdering controllerOrdering)
        {
            ControllerOrdering = controllerOrdering;
        }
    }
}
