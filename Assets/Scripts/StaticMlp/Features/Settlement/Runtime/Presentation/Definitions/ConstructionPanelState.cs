using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct ConstructionPanelState
    {
        public EntityGID Target;
        public string DisplayName;
        public ConstructionPhase Phase;
        public FixedList512Bytes<ConstructionResourceViewEntry> Resources;
        public float Progress01;
    }
}
