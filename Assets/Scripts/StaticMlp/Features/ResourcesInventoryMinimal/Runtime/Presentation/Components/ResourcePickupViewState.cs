using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public struct ResourcePickupViewState : IViewComponent
    {
        public ushort ResourceId;
        public int Amount;
        public bool IsMagnetized;
    }
}
