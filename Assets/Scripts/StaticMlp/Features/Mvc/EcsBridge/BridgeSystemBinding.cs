using System;

namespace Code.EcsUi.Mvc
{
    /// <summary>
    /// Connects controller lifecycle callbacks to a presentation-sync system.
    /// This module is intended for client UX state synchronization only.
    /// </summary>
    public sealed class BridgeSystemBinding<TSystem, TController> : IMvcControllerModule
        where TSystem : class, IControllerEcsBridgeSystem<TController>
        where TController : class, IController
    {
        private readonly TController controller;
        private TSystem presentationSyncSystem;

        public BridgeSystemBinding(TController controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public BridgeSystemBinding(TController controller, TSystem system)
            : this(controller)
        {
            BindPresentationSyncSystem(system);
        }

        public void BindPresentationSyncSystem(TSystem systemInstance)
        {
            if (presentationSyncSystem != null)
                throw new ArgumentException("System is already injected.", nameof(systemInstance));

            presentationSyncSystem = systemInstance ?? throw new ArgumentNullException(nameof(systemInstance));
            presentationSyncSystem.Bind(controller);

            if (controller.State != ControllerState.ViewFocused)
                return;

            presentationSyncSystem.Activate();
            presentationSyncSystem.SyncOnce();
        }

        void IMvcControllerModule.OnFocus()
        {
            if (presentationSyncSystem == null)
                return;

            presentationSyncSystem.Activate();
            presentationSyncSystem.SyncOnce();
        }

        void IMvcControllerModule.OnBlur()
        {
            presentationSyncSystem?.Deactivate();
        }

        void IMvcControllerModule.OnViewShow()
        {
            if (presentationSyncSystem == null)
                return;

            presentationSyncSystem.Activate();
            presentationSyncSystem.SyncOnce();
        }

        void IMvcControllerModule.OnViewHide()
        {
            presentationSyncSystem?.Deactivate();
        }
    }
}
