using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;

namespace StaticMlp.Features.Settlement
{
    public struct CampCorePanelState
    {
        public EntityGID Target;
        public string DisplayName;
        public ConstructionPhase Phase;
        public float Progress01;
    }
}
