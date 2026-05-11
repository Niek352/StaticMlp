using System;
using FFS.Libraries.StaticEcs;

namespace Code.EcsUi.Mvc
{
    /// <summary>
    /// Base class for presentation-sync systems that stay active only while their controller is active.
    /// Use this to mirror prepared ECS presentation state into the bound controller or view.
    /// Do not use it for gameplay, validation, or domain state transitions.
    /// </summary>
    public abstract class ControllerEcsBridgeSystem<TController> : ISystem, IControllerEcsBridgeSystem<TController>
        where TController : class, IController
    {
        private bool isActive;
        private bool isBound;

        protected TController Controller { get; private set; }

        public void Update()
        {
            if (!isBound)
                return;

            if (Controller.State == ControllerState.ViewHidden)
            {
                isActive = false;
                return;
            }

            if (!isActive)
                isActive = true;

            EnsureBound();
            SyncPresentation();
        }

        public void Bind(TController controller)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            isBound = true;
        }

        public void Activate()
        {
            isActive = true;
        }

        public void Deactivate()
        {
            isActive = false;
        }

        public void SyncOnce()
        {
            EnsureBound();
            SyncPresentation();
        }

        protected abstract void SyncPresentation();

        private void EnsureBound()
        {
            if (!isBound)
                throw new InvalidOperationException($"{GetType().Name} must be bound to a controller before use.");
        }
    }
}
