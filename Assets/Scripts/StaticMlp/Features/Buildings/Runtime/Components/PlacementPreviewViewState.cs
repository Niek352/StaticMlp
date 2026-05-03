using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.Buildings
{
    public struct PlacementPreviewViewState : IViewComponent
    {
        public bool IsValid;
        public PlacementInvalidReason InvalidReason;
    }
}
