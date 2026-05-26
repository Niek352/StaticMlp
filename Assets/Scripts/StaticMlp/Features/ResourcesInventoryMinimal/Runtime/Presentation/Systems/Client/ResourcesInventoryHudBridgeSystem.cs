using Code.EcsUi.Mvc;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryHudBridgeSystem : ControllerEcsBridgeSystem<ResourcesInventoryHudController>
    {
        protected override void SyncPresentation()
        {
            var presentation = ResourcesInventoryHudPresentationBuilder.Build();
            Controller.Apply(in presentation);
        }
    }
}
