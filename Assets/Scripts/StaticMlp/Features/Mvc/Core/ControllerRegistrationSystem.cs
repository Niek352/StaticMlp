using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace Code.EcsUi.Mvc
{
    public sealed class ControllerRegistrationSystem<TView, TController> : ISystem
        where TView : IView
        where TController : class, IController<TView, ControllerNoData>
    {
        private readonly TController _controller;

        public ControllerRegistrationSystem(TController controller)
        {
            _controller = controller;
        }

        public void Init()
        {
            var manager = CW.GetResource<MvcManagerResource>().Manager;
            manager.RegisterController(_controller);
        }
    }
}
