using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Components.Buildings;

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
