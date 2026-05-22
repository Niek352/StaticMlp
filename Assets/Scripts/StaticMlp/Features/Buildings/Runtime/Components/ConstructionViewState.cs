using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Settlement;
using Unity.Collections;

namespace StaticMlp.Features.Buildings
{
    public struct ConstructionViewState : IViewComponent
    {
        public ConstructionPhase Phase;
        public FixedList512Bytes<ConstructionResourceViewEntry> Resources;
        public float Progress01;
    }
}
