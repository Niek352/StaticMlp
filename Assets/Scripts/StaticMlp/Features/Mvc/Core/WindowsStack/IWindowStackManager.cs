using System;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public interface IWindowStackManager : IDisposable
    {
        (IController controller, int orderInLayer) TopMostPopup { get; }

        IController CurrentFullscreenController { get; }

        PopupPushInfo PushPopup(IController controller);

        FullscreenPushInfo PushFullscreen(IController controller);

        void PopFullscreen(IController controller);

        PersistentPushInfo PushPersistent(IController controller);

        void RemovePersistent(IController controller);

        OverlayPushInfo PushOverlay(IController controller);

        void PopOverlay(IController controller);

        PopupPopInfo PopPopup(IController controller, bool shouldGracefullyClose = true);

        UniTaskCompletionSource GetControllerClosure(IController controller);

        bool TryCloseTopClosable();

        bool TryClose(IController controller);
    }
}
