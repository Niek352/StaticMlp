using Code.EcsUi.Mvc;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryHudController : ControllerBase<ResourcesInventoryHudView>
    {
        public ResourcesInventoryHudController(
            ViewFactoryMethod<ResourcesInventoryHudView> viewFactory,
            ResourcesInventoryHudBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ResourcesInventoryHudBridgeSystem, ResourcesInventoryHudController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 20;

        public void Apply(in ResourcesInventoryHudPresentation presentation)
        {
            View.Render(in presentation);
        }
    }
}
