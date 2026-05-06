namespace Code.EcsUi.Mvc
{
    /// <summary>
    /// Presentation-only adapter between an active MVC controller and ECS-driven screen state.
    /// Do not place gameplay or domain decisions in implementations of this contract.
    /// </summary>
    public interface IControllerEcsBridgeSystem
    {
        void Activate();

        void Deactivate();

        void SyncOnce();
    }

    /// <summary>
    /// Binds a presentation sync adapter to a concrete controller instance.
    /// Implementations should read prepared ECS presentation state and apply it to the controller/view.
    /// </summary>
    public interface IControllerEcsBridgeSystem<in TController> : IControllerEcsBridgeSystem
    {
        void Bind(TController controller);
    }
}
