using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Buildings
{
    public struct ConstructionViewState : IViewComponent
    {
        public ConstructionPhase Phase;
        public int WoodRequired;
        public int StoneRequired;
        public int WoodDelivered;
        public int StoneDelivered;
        public float Progress01;
    }
}
